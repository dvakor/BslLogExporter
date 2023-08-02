using LogExporter.App.Helpers;
using LogExporter.App.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace LogExporter;

public class LogsReaderBackgroundService : BackgroundService
{
    private static readonly AsyncPolicy Policy = HelperMethods
        .CreateRetryAsyncPolicy<Exception>(5, 1000);
    
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
                await Policy.ExecuteAsync(async () => 
                    await _processingManager.PublishLogsAsync(stoppingToken));
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation(e, "Операция чтения прервана");
            }
        }
        
        _logger.LogInformation("Служба чтения логов остановлена");
    }
}