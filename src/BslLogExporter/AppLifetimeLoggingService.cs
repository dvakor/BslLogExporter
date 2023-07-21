using LogExporter.App.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogExporter;

public class AppLifetimeLoggingService : BackgroundService
{
    private readonly ILogger<AppLifetimeLoggingService> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public AppLifetimeLoggingService(
        ILogger<AppLifetimeLoggingService> logger,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _lifetime = lifetime;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Запуск приложения");
        
        await _lifetime.ApplicationStarted.WaitForCancellation();
        
        _logger.LogInformation("Приложение запущено в режиме: {Mode}",
            AppModeHelper.GetAppRunMode());

        await stoppingToken.WaitForCancellation();
        
        _logger.LogInformation("Завершение работы приложения");
    }
}