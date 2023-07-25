using BslLogExporter.Tests.Helpers;
using LogExporter.App.Exporters;
using LogExporter.App.Sources;
using Microsoft.Extensions.Configuration;

namespace BslLogExporter.Tests.Stubs;

public static class SutFactory
{
    public static LogSourcesSnapshot CreateSource<TFactory>(object sourceSettings, Func<TFactory> factory) where TFactory : ILogSourceFactory
    {
        var settings = new
        {
            Src = sourceSettings
        };
        
        var confRoot = 
            new ConfigurationBuilder()
                .AddObject(settings)
                .Build();

        var section = confRoot.GetSection(nameof(settings.Src));
        
        var sources = factory().CreateSources(section);

        return sources;
    }

    public static ILogExporter CreateExporter<TFactory>(object exporterSettings, Func<TFactory> factory)
        where TFactory : ILogExporterFactory
    {
        var settings = new
        {
            Exporter = exporterSettings
        };
        
        var confRoot = 
            new ConfigurationBuilder()
                .AddObject(settings)
                .Build();
        
        var section = confRoot.GetSection(nameof(settings.Exporter));

        var exporter = factory().CreateExporter(section);

        return exporter;
    }
}