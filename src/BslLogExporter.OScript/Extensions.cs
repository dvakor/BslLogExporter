using LogExporter.App.Exporters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BslLogExporter.OScript;

public static class Extensions
{
    public static void AddOScriptExporter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OScriptSettings>(configuration.GetSection("OneScript"));
        services.AddSingleton<ILogExporterFactory, OScriptExporterFactory>();
    }
}