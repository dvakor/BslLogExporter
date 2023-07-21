using Microsoft.Extensions.Configuration;

namespace LogExporter.App.Exporters;

public abstract class AbstractExporterFactory<TArgs> : ILogExporterFactory
{
    public abstract string TypeName { get; }
    
    public ILogExporter CreateExporter(IConfigurationSection args)
    {
        var typedArgs = args.Get<TArgs>()!;
        return CreateExporter(typedArgs);
    }

    protected abstract ILogExporter CreateExporter(TArgs args);
}