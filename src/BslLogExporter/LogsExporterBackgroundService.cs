using LogExporter.App.Helpers;
using LogExporter.App.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace LogExporter;

public class LogsExporterBackgroundService : BackgroundService
{
    private readonly AsyncPolicy _policy;
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

        _policy = HelperMethods
            .CreateInfinityRetryPolicy<Exception>(1000, OnRetry);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _lifetime.ApplicationStarted.WaitForCancellation();
        
        _logger.LogInformation("Служба обработки логов запущена");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _policy.ExecuteAsync(ct
                    => _processingManager.ProcessLogsAsync(ct), stoppingToken);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation(e, "Операция обработки логов прервана");
            }
        }
        
        _logger.LogInformation("Служба обработки логов остановлена");
    }
    
    private void OnRetry(Exception exception, TimeSpan currentTimeout)
    {
        _logger.LogError(exception, "Ошибка обработки логов, текущий таймаут {Timeout}", currentTimeout);
    }
}