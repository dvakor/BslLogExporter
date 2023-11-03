using LogExporter.App.Helpers;
using LogExporter.App.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace LogExporter;

public class LogsReaderBackgroundService : BackgroundService
{
    private readonly AsyncPolicy _policy;
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
        
        _policy = HelperMethods
            .CreateInfinityRetryPolicy<Exception>(1000, OnRetry);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _lifetime.ApplicationStarted.WaitForCancellation();
        
        _logger.LogInformation("Служба чтения логов запущена");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _policy.ExecuteAsync(ct 
                    => _processingManager.PublishLogsAsync(ct), stoppingToken);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation(e, "Операция чтения прервана");
            }
        }
        
        _logger.LogInformation("Служба чтения логов остановлена");
    }
    
    private void OnRetry(Exception exception, TimeSpan currentTimeout)
    {
        _logger.LogError(exception, "Ошибка чтения логов, текущий таймаут {Timeout}", currentTimeout);
    }
}