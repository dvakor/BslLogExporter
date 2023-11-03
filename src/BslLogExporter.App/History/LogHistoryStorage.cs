using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogExporter.App.History;

public sealed class LogHistoryStorage : ILogHistoryStorage, IAsyncDisposable
{
    private const int SavePeriod = 1000;
    
    private readonly ILogger<LogHistoryStorage> _logger;
    private readonly FileStream _fs;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Stopwatch _stopwatch = new();
    
    private PositionsData? _history;
    private bool _hasChanges;

    public LogHistoryStorage(
        IOptions<HistorySettings> settings,
        ILogger<LogHistoryStorage> logger)
    {
        _logger = logger;
        var historyFile = settings.Value.HistoryFile ?? "./history.json";
        _fs = File.Open(historyFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        _stopwatch.Start();
    }
    
    public async ValueTask SaveHistoryAsync(string sourceName, string fileName, long position)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            _history ??= await LoadHistoryAsync();
            _history.Add(sourceName, fileName, position);

            if (_stopwatch.ElapsedMilliseconds > SavePeriod)
            {
                _hasChanges = false;
                await SaveHistoryAsync();
                _stopwatch.Reset();
            }
            else
            {
                _hasChanges = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask<HistoryRecord> GetLastPositionAsync(string sourceName)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            _history ??= await LoadHistoryAsync();
            return _history.GetLastPosition(sourceName);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveHistoryAsync()
    {
        if (_history is null)
        {
            return;
        }

        _logger.LogDebug("История обновлена");
        
        _fs.SetLength(0);

        await JsonSerializer.SerializeAsync(_fs, _history, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        await _fs.FlushAsync();
    }

    private async ValueTask<PositionsData> LoadHistoryAsync()
    {
        _fs.Seek(0, SeekOrigin.Begin);

        if (_fs.Length == 0)
        {
            return new PositionsData();
        }
        
        var positions = await JsonSerializer.DeserializeAsync<PositionsData>(_fs);

        return positions ?? new PositionsData();
    }
    
    private class PositionsData
    {
        [JsonInclude]
        public Dictionary<string, HistoryRecord> Positions { get; private set; } = new();

        public void Add(string sourceName, string fileName, long filePosition)
        {
            var record = new HistoryRecord
            {
                File = fileName,
                Position = filePosition
            };

            Positions[sourceName] = record;
        }

        public HistoryRecord GetLastPosition(string sourceFile)
        {
            return Positions.TryGetValue(sourceFile, out var record) 
                ? record 
                : HistoryRecord.Empty;
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_hasChanges)
        {
            await _semaphore.WaitAsync();
            
            try
            {
                await SaveHistoryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        _semaphore.Dispose();
        await _fs.DisposeAsync();
    }
}