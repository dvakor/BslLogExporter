using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace LogExporter.App.Exporters;

public sealed class LogExportersManager
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ILogExporterFactory> _factories;

    public LogExportersManager(
        IConfiguration configuration,
        IEnumerable<ILogExporterFactory> factories)
    {
        _factories = factories;
        _configuration = configuration;
    }

    public LogExportersSnapshot GetExporters() 
        => CreateExporters();

    public void Validate()
    {
        try
        {
            using var _ = CreateExporters();
        }
        catch (Exception e)
        {
            throw new ExportersValidationException(e);
        }
    }

    private LogExportersSnapshot CreateExporters()
    {
        var exportersConfiguration = _configuration.GetSection("Exporters");

        var exporters = new List<KeyValuePair<string, ILogExporter>>();

        foreach (var section in exportersConfiguration.GetChildren())
        {
            var type = section.GetSection("Type").Get<string>();
            var pattern = section.GetSection("SourceFilter").Get<string>();
            
            Guard.Against.NullOrWhiteSpace(type);
            Guard.Against.NullOrWhiteSpace(pattern);
            
            var factory = FindFactory(type);

            var exporter = factory.CreateExporter(section.GetSection("Args"));
            
            exporters.Add(new KeyValuePair<string, ILogExporter>(pattern, exporter));
        }
        
        return new LogExportersSnapshot
        {
            ChangeToken = _configuration.GetReloadToken(),
            Exporters = exporters
        };
    }

    private ILogExporterFactory FindFactory(string type)
    {
        var candidates = _factories
            .Where(x => x.TypeName.Equals(type, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return candidates.Count switch
        {
            1 => candidates[0],
            > 1 => throw new AmbiguousMatchException(type),
            _ => throw new UnknownExporterTypeException(type)
        };
    }
}