using LogExporter.App.Exporters;
using Microsoft.Extensions.Configuration;

namespace BslLogExporter.Tests.Stubs.Exporters;

public class FakeExporterFactory : ILogExporterFactory
{
    public string TypeName => "Fake";
    
    public ILogExporter CreateExporter(IConfigurationSection args)
    {
        return new FakeExporter();
    }
}