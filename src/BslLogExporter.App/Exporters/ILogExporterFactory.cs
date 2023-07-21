using Microsoft.Extensions.Configuration;

namespace LogExporter.App.Exporters;

public interface ILogExporterFactory
{
    string TypeName { get; }

    ILogExporter CreateExporter(IConfigurationSection args);
}