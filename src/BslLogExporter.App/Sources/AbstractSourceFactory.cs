using Microsoft.Extensions.Configuration;

namespace LogExporter.App.Sources;

public abstract class AbstractSourceFactory<TArgs> : ILogSourceFactory
{
    public abstract string TypeName { get; }

    public LogSourcesSnapshot CreateSources(IConfigurationSection args)
    {
        var typedArgs = args.Get<TArgs>();
        return CreateSources(typedArgs);
    }

    protected abstract LogSourcesSnapshot CreateSources(TArgs args);
}