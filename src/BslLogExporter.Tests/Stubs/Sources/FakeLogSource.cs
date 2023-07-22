using System.Collections.Generic;
using System.Threading;
using LogExporter.App.History;
using LogExporter.App.Sources;
using LogExporter.Core.LogReader;
using Microsoft.Extensions.Options;

namespace BslLogExporter.Tests.Stubs.Sources;

public class FakeLogSource : ILogSource
{
    private readonly IOptionsMonitor<FakeSourceSettings> _settings;
    
    public string Name { get; }

    public FakeLogSource(IOptionsMonitor<FakeSourceSettings> settings, string name)
    {
        _settings = settings;
        Name = name;
    }

    public IEnumerable<BslLogEntry> GetLogs(CancellationToken token)
    {
        return _settings.CurrentValue.LogsProvider(Name, token);
    }

    public void ForwardTo(HistoryRecord record)
    {
        // NoOp
    }
}