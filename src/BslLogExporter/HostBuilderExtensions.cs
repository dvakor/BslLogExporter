using Ardalis.GuardClauses;
using BslLogExporter.CsScript;
using BslLogExporter.OScript;
using LogExporter.App.Exporters;
using LogExporter.App.History;
using LogExporter.App.Processing;
using LogExporter.App.Sources;
using LogExporter.App.Sources.Cluster;
using LogExporter.App.Sources.Folder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LogExporter
{
    public static class HostBuilderExtensions
    {
        public static void SetupEnvironmentAndConfiguration(this HostApplicationBuilder host, string[] args)
        {
            host.Configuration.Sources.Clear();

            host.Configuration.AddCommandLine(args);
            
            host.Configuration.AddEnvironmentVariables("BslLogExporter");

            SetupEnvironment(host);

            SetupConfigurationFiles(host);
        }

        private static void SetupEnvironment(HostApplicationBuilder host)
        {
            var envConfig = host.Configuration.Get<EnvConfig>();

            if (string.IsNullOrEmpty(envConfig?.WorkingDir))
            {
                return;
            }
            
            Directory.SetCurrentDirectory(envConfig.WorkingDir);
            host.Environment.ContentRootPath = envConfig.WorkingDir;
        }

        public static void SetupLogging(this HostApplicationBuilder host)
        {
            host.Logging.ClearProviders();
            
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(host.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            host.Logging.AddSerilog(logger);
        }

        public static void SetupApplication(this HostApplicationBuilder host)
        {
            host.Services.AddSingleton<LogSourcesManager>();
            host.Services.AddSingleton<ILogSourceFactory, ClusterSourceFactory>();
            host.Services.AddSingleton<ILogSourceFactory, FolderSourceFactory>();
            host.Services.AddHostedService<LogsReaderBackgroundService>();

            host.Services.AddSingleton<LogExportersManager>();
            host.Services.AddCsScriptExporter(host.Configuration);
            host.Services.AddOScriptExporter(host.Configuration);
            host.Services.AddHostedService<LogsExporterBackgroundService>();
            
            host.Services.AddSingleton<ILogHistoryStorage, LogHistoryStorage>();
            host.Services.Configure<HistorySettings>(host.Configuration.GetSection("History"));
            
            host.Services.AddSingleton<LogsBuffer>();
            host.Services.AddSingleton<LogsProcessor>();
            host.Services.Configure<ProcessingConfiguration>(host.Configuration.GetSection("Processing"));
            
            host.Services.AddWindowsService();
            host.Services.AddHostedService<AppLifetimeLoggingService>();
        }
        
        private static void SetupConfigurationFiles(HostApplicationBuilder host)
        {
            var binLocation = Path.GetDirectoryName(typeof(HostBuilderExtensions).Assembly.Location);

            binLocation = Guard.Against.Null(binLocation)!;
            
            var configName = AppModeHelper.GetAppRunMode();
            
            host.Configuration.AddJsonFile(Path.Combine(
                    binLocation, 
                    AppSettingsFileName()),
                false, false);
            
            host.Configuration.AddJsonFile(Path.Combine(
                    binLocation, 
                    AppSettingsFileName(configName)),
                false, false);
            
            host.Configuration.AddJsonFile(Path.Combine(
                    host.Environment.ContentRootPath, 
                    "AppConfiguration.json"), 
                false, true);
            
            static string AppSettingsFileName(string? prefix = default)
            { 
                const string appSettings = "appsettings";
                const string jsonExt = "json";
                
                return string.IsNullOrWhiteSpace(prefix) 
                    ? $"{appSettings}.{jsonExt}" 
                    : $"{appSettings}.{prefix}.{jsonExt}";
            }
        }
    }
}