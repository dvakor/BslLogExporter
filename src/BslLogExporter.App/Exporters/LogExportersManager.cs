using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogExporter.App.Exporters;

public sealed class LogExportersManager : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ILogExporterFactory> _factories;
    private readonly ILogger<LogExportersManager> _logger;
    private readonly object _lockObj = new();

    private LogExportersSnapshot? _exportersSnapshot;

    public LogExportersManager(
        IConfiguration configuration,
        IEnumerable<ILogExporterFactory> factories,
        ILogger<LogExportersManager> logger)
    {
        _factories = factories;
        _configuration = configuration;
        _logger = logger;
    }

    public LogExportersSnapshot GetExporters()
    {
        lock (_lockObj)
        {
            if (_exportersSnapshot is null or { ChangeToken.HasChanged: true })
            {
                LoadExporters();
            }
        
            return _exportersSnapshot!;
        }
    }

    public void Validate()
    {
        lock (_lockObj)
        {
            try
            {
                LoadExporters();
            }
            catch (Exception e)
            {
                throw new ExportersValidationException(e);
            }
        }
    }

    private void LoadExporters()
    {
        var exportersConfiguration = _configuration.GetSection("Exporters");

        var exporters = new List<KeyValuePair<string, ILogExporter>>();
        var exportersNames = new List<string>();

        foreach (var section in exportersConfiguration.GetChildren())
        {
            var type = section.GetSection("Type").Get<string>();
            var pattern = section.GetSection("SourceFilter").Get<string>();
            
            exportersNames.Add($"{type}:{pattern}");

            Guard.Against.NullOrWhiteSpace(type);
            Guard.Against.NullOrWhiteSpace(pattern);
            
            var factory = FindFactory(type);

            var exporter = factory.CreateExporter(section.GetSection("Args"));
            
            exporters.Add(new KeyValuePair<string, ILogExporter>(pattern, exporter));
        }
        
        _logger.LogInformation("Список экспортеров обновлен {@Exporters}", exportersNames);

        _exportersSnapshot = new LogExportersSnapshot
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

    public void Dispose()
    {
        _exportersSnapshot?.Dispose();
    }
}