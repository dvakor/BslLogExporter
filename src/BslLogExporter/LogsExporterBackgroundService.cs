using LogExporter.App.Helpers;
using LogExporter.App.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace LogExporter;

public class LogsExporterBackgroundService : BackgroundService
{
    private static readonly AsyncPolicy Policy = HelperMethods
        .CreateRetryAsyncPolicy<Exception>(5, 1000);
    
    private readonly LogsProcessor _processingManager;
    private readonly ILogger<LogsExporterBackgroundService> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public LogsExporterBackgroundService(
        LogsProcessor processingManager,
        ILogger<LogsExporterBackgroundService> logger,
        IHostApplicationLifetime lifetime)
    {
        _processingManager = processingManager;
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _lifetime.ApplicationStarted.WaitForCancellation();
        
        _logger.LogInformation("Служба обработки логов запущена");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Policy.ExecuteAsync(async () =>
                    await _processingManager.ProcessLogsAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // NoOp
            }
        }
        
        _logger.LogInformation("Служба обработки логов остановлена");
    }
}