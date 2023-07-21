using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Sources;

public sealed class LogSourcesManager : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ILogSourceFactory> _factories;
    private readonly ILogger<LogSourcesManager> _logger;
    private readonly object _lockObj = new();
    
    private LogSourcesSnapshot? _sources;
    
    public LogSourcesManager(
        IConfiguration configuration,
        IEnumerable<ILogSourceFactory> factories,
        ILogger<LogSourcesManager> logger)
    {
        _configuration = configuration;
        _factories = factories;
        _logger = logger;
    }

    public LogSourcesSnapshot GetSources(CancellationToken token)
    {
        lock (_lockObj)
        {
            if (_sources is null or { ChangeToken.HasChanged: true })
            {
                LoadSources();
            }

            return _sources!;
        }
    }
    
    public void Validate()
    {
        lock (_lockObj)
        {
            try
            {
                LoadSources();
            }
            catch (Exception e)
            {
                throw new SourcesValidationException(e);
            }
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

    private void LoadSources()
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
            var factory = FindFactory(type);
            var sourceFromCurrentSection = factory.CreateSources(
                section.GetSection("Args"));
            
            sources.AddRange(sourceFromCurrentSection.Sources);
            tokens.Add(sourceFromCurrentSection.ChangeToken);
        }

        var sourceNames = sources
            .Select(x => x.Name)
            .ToList();
        
        _logger.LogInformation("Список источников обновлен {@Sources}", sourceNames);

        _sources = new LogSourcesSnapshot
        {
            Sources = sources,
            ChangeToken = new CompositeChangeToken(tokens)
        };
    }

    public void Dispose()
    {
        _sources?.Dispose();
    }
}