using Microsoft.Extensions.Configuration;

namespace LogExporter.App.Sources;

public interface ILogSourceFactory
{
    string TypeName { get; }

    LogSourcesSnapshot CreateSources(IConfigurationSection args);
}