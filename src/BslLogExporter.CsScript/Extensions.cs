using Dotnet.Script.DependencyModel.Logging;
using LogExporter.App.Exporters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BslLogExporter.CsScript;

public static class Extensions
{
    public static void AddCsScriptExporter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CsScriptSettings>(configuration.GetSection("CsScript"));
        services.AddSingleton<ILogExporterFactory, CsScriptExporterFactory>();
        services.AddSingleton<CsScriptCompiler>();
        services.AddSingleton<CsScriptExecutor>();
    }
    
    internal static Logger CreateCsLogger(this ILoggerFactory factory, Type type)
    {
        var logger = factory.CreateLogger(type);
        return (level, message, exception) => logger.Log(ToMsLogLevel(level), exception, message);
    }

    private static LogLevel ToMsLogLevel(Dotnet.Script.DependencyModel.Logging.LogLevel level)
    {
        return level switch
        {
            Dotnet.Script.DependencyModel.Logging.LogLevel.Trace => LogLevel.Trace,
            Dotnet.Script.DependencyModel.Logging.LogLevel.Debug => LogLevel.Debug,
            Dotnet.Script.DependencyModel.Logging.LogLevel.Info => LogLevel.Information,
            Dotnet.Script.DependencyModel.Logging.LogLevel.Warning => LogLevel.Warning,
            Dotnet.Script.DependencyModel.Logging.LogLevel.Error => LogLevel.Error,
            Dotnet.Script.DependencyModel.Logging.LogLevel.Critical => LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}