using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Sources;

public sealed class LogSourcesManager
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ILogSourceFactory> _factories;

    public LogSourcesManager(
        IConfiguration configuration,
        IEnumerable<ILogSourceFactory> factories)
    {
        _configuration = configuration;
        _factories = factories;
    }

    public LogSourcesSnapshot GetSources() => CreateSources();
    
    public void Validate()
    {
        try
        {
            using var _ = CreateSources();
        }
        catch (Exception e)
        {
            throw new SourcesValidationException(e);
        }
    }

    private ILogSourceFactory FindFactory(string type)
    {
        var candidates = _factories
            .Where(x => x.TypeName.Equals(type, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return candidates.Count switch
        {
            1 => candidates[0],
            > 1 => throw new AmbiguousMatchException(),
            _ => throw new Exception()
        };
    }

    private LogSourcesSnapshot CreateSources()
    {
        var tokens = new List<IChangeToken>
        {
            _configuration.GetReloadToken()
        };
        
        var sources = new List<ILogSource>();

        var sourceConfiguration = _configuration.GetSection("Sources");

        foreach (var section in sourceConfiguration.GetChildren())
        {
            var type = section.GetSection("Type").Get<string>();

            Guard.Against.Null(type);
            
            var factory = FindFactory(type);
            var sourceFromCurrentSection = factory.CreateSources(
                section.GetSection("Args"));
            
            sources.AddRange(sourceFromCurrentSection.Sources);
            tokens.Add(sourceFromCurrentSection.ChangeToken);
        }

        return new LogSourcesSnapshot
        {
            Sources = sources,
            ChangeToken = new CompositeChangeToken(tokens)
        };
    }
}