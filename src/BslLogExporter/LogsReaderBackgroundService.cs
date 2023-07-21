using LogExporter.App.Helpers;
using LogExporter.App.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogExporter;

public class LogsReaderBackgroundService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly LogsProcessor _processingManager;
    private readonly ILogger<LogsReaderBackgroundService> _logger;
    
    public LogsReaderBackgroundService(
        LogsProcessor processingManager,
        ILogger<LogsReaderBackgroundService> logger,
        IHostApplicationLifetime lifetime)
    {
        _processingManager = processingManager;
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _lifetime.ApplicationStarted.WaitForCancellation();
        
        _logger.LogInformation("Служба чтения логов запущена");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _processingManager.PublishLogsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // NoOp;
            }
        }
        
        _logger.LogInformation("Служба чтения логов остановлена");
    }
}