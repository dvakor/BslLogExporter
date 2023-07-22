using System.Collections.Generic;
using LogExporter.App.Sources;
using Microsoft.Extensions.Options;

namespace BslLogExporter.Tests.Stubs.Sources;

public class FakeLogsSourceFactory : AbstractSourceFactory<FakeLogArgs>
{
    private readonly IOptionsMonitor<FakeSourceSettings> _settings;
    
    public override string TypeName => "Fake";

    public FakeLogsSourceFactory(IOptionsMonitor<FakeSourceSettings> settings)
    {
        _settings = settings;
    }

    protected override LogSourcesSnapshot CreateSources(FakeLogArgs args)
    {
        return new LogSourcesSnapshot
        {
            Sources = new List<ILogSource>
            {
                new FakeLogSource(_settings, args.Name)
            },
            ChangeToken = _settings.CurrentValue.ChangeToken()
        };
    }
}