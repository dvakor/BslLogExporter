using LogExporter.App.Exporters;
using LogExporter.App.Helpers;
using LogExporter.App.History;
using LogExporter.App.Sources;
using Microsoft.Extensions.Logging;

namespace LogExporter.App.Processing;

public class LogsProcessor
{
    private readonly LogsBuffer _buffer;
    private readonly LogSourcesManager _sourcesManager;
    private readonly LogExportersManager _exportersManager;
    private readonly ILogHistoryStorage _historyStorage;
    private readonly ILogger<LogsProcessor> _logger;

    public LogsProcessor(
        LogsBuffer buffer,
        LogSourcesManager sourcesManager,
        LogExportersManager exportersManager,
        ILogHistoryStorage historyStorage,
        ILogger<LogsProcessor> logger)
    {
        _buffer = buffer;
        _sourcesManager = sourcesManager;
        _exportersManager = exportersManager;
        _historyStorage = historyStorage;
        _logger = logger;
    }
    
    public async Task PublishLogsAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }
        
        using var snapshot = _sourcesManager.GetSources();
        
        if (token.IsCancellationRequested 
            || !await EnsureSourcesAsync(snapshot, token))
        {
            return;
        }
        
        _logger.LogInformation("Начало чтения логов");
        
        await using var changeTokenAwaiter = ChangeTokenAwaiter.Create(snapshot.ChangeToken, token: token);

        await PrepareReadersAsync(snapshot, changeTokenAwaiter.Token);
        
        var workerTasks = snapshot.Sources
            .Select(source => new LogPublisherWorker(source, _buffer))
            .Select(worker => worker.Start(changeTokenAwaiter.Token))
            .ToList();
        
        await Task.WhenAll(workerTasks);
        
        _logger.LogInformation("Завершение чтения логов");
    }

    public async Task ProcessLogsAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }
        
        using var snapshot = _exportersManager.GetExporters();

        if (token.IsCancellationRequested
            || !await EnsureExportersAsync(snapshot, token))
        {
            return;
        }
        
        _logger.LogInformation("Начало экспорта логов");
        
        await using var changeTokenAwaiter = ChangeTokenAwaiter.Create(snapshot.ChangeToken, token: token);
        
        await foreach (var portion in _buffer.GetLogsAsync(changeTokenAwaiter.Token))
        {
            await ExportLogsAsync(snapshot, portion);
        }
        
        _logger.LogInformation("Завершение экспорта логов");
    }

    private async Task ExportLogsAsync(LogExportersSnapshot snapshot, SourceLogPortion portion)
    {
        var exporterForPortion = snapshot.GetExportersFor(portion.SourceName);

        if (!exporterForPortion.Any())
        {
            _logger.LogWarning("Экспортер для источника: {Source} не найден", portion.SourceName);
            return;
        }

        foreach (var exporter in exporterForPortion)
        {
            await exporter.ExportLogsAsync(portion);
        }

        var maxPositions = portion.Entries
            .GroupBy(x => x.FileName)
            .Select(x => new
            {
                FileName = x.Key,
                MaxPosition = x.Max(e => e.Position)
            })
            .ToList();

        _logger.LogDebug("Экспортированы логи источика {Source} в количестве {Count}",
            portion.SourceName, portion.Entries.Count);

        foreach (var position in maxPositions)
        {
            await _historyStorage.SaveHistoryAsync(
                portion.SourceName,
                position.FileName,
                position.MaxPosition);
        }
    }
    
    private async ValueTask<bool> EnsureSourcesAsync(LogSourcesSnapshot snapshot, CancellationToken token)
    {
        if (!snapshot.Sources.Any())
        {
            _logger.LogInformation("Источники не найдены, ожидание добавления источников");
            await ChangeTokenAwaiter.WaitForTokenChange(snapshot.ChangeToken, token: token);
            return false;
        }
        
        var sourcesNames = snapshot
            .Sources
            .Select(x => x.Name);

        _logger.LogInformation("Загружены источники: {Sources}", sourcesNames.ToList());

        return true;
    }
    
    private async ValueTask<bool> EnsureExportersAsync(LogExportersSnapshot snapshot, CancellationToken token)
    {
        if (!snapshot.Exporters.Any())
        {
            _logger.LogInformation("Экспортеры не найдены, ожидание добавления экспортеров");
            await ChangeTokenAwaiter.WaitForTokenChange(snapshot.ChangeToken, token: token);
            return false;
        }

        return true;
    }

    private async ValueTask PrepareReadersAsync(
        LogSourcesSnapshot sourcesSnapshot, CancellationToken token)
    {
        foreach (var source in sourcesSnapshot.Sources)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            var record = await _historyStorage.GetLastPositionAsync(source.Name);

            if (!record.IsEmpty())
            {
                _logger.LogDebug("Чтение логов {Source} с позиции {Position} из файла {File}", 
                    source.Name, record.Position, record.File);
            }
            
            try
            {
                source.ForwardTo(record);
            }
            catch (FileNotFoundException e)
            {
                _logger.LogError(e, "Не удалось перемотать лог к позиции {Position} в файле {File}, тк файла больше нет", 
                    record.Position, record.File);
            }
        }
    }
}