using LogExporter.App.Exporters;
using Microsoft.Extensions.Configuration;

namespace BslLogExporter.Tests.Stubs.Exporters;

public class FakeExporterFactory : ILogExporterFactory
{
    private static readonly FakeExporter _exporter = new();
    
    public string TypeName => "Fake";

    public ILogExporter CreateExporter(IConfigurationSection args)
    {
        return _exporter;
    }
}