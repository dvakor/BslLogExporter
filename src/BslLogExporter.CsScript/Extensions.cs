using LogExporter.App.Exporters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BslLogExporter.CsScript;

public static class Extensions
{
    public static void AddCsScriptExporter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CsScriptSettings>(configuration.GetSection("CsScript"));
        services.AddSingleton<ILogExporterFactory, CsScriptExporterFactory>();
        services.AddSingleton<CsScriptCompiler>();
    }
}