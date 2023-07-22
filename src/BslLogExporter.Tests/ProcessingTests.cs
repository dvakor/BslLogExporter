using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BslLogExporter.Tests.Host;
using BslLogExporter.Tests.Stubs.Exporters;
using BslLogExporter.Tests.Stubs.Sources;
using LogExporter.App.Exporters;
using LogExporter.App.Processing;
using LogExporter.Core.LogReader;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace BslLogExporter.Tests;

public class ProcessingTests
{
    private readonly ITestOutputHelper _helper;

    public ProcessingTests(ITestOutputHelper helper)
    {
        _helper = helper;
    }
    
    [Theory]
    [InlineData(2, 10, 40)]
    [InlineData(1, 100, 20)]
    public async Task Should_Process_Logs(
        int readersCount, int bufferSize,
        int totalLogs)
    {
        var current = 0;
        
        await using var host = TestHostBuilder.New()
            .WithFakeLogsExporter()
            .WithFakeLogsSource(settings =>
            {
                settings.LogsProvider = LogsProvider;
            })
            .WithTestOutputLogger(_helper)
            .WithSettings(GetSettingsObject(readersCount, bufferSize))
            .WithInMemoryHistoryStorage()
            .Build();

        await host.StartHost();
        
        var manager = host.Services.GetRequiredService<LogExportersManager>();
        using var snapshot = manager.GetExporters();
        var exporter = (FakeExporter)snapshot.GetExportersFor("FakeSource1").Single();
        
        await Task.Delay(3000);

        await host.StopHost();
        
        Assert.True(exporter.Portions.Count > 0);

        IEnumerable<BslLogEntry> LogsProvider(string sourceName, CancellationToken token)
        {
            if (current > 0)
            {
                current += 1;
            }
            
            for (var i = current; i < totalLogs; i++)
            {
                yield return new BslLogEntry
                {
                    FileName = "Generated",
                    Position = i + 1
                };

                current += 1;
            }
        }
    }

    private static object GetSettingsObject(int readersCount, int bufferSize)
    {
        var settings = new
        {
            Processing = new ProcessingConfiguration
            {
                BufferSize = bufferSize,
                BufferTimeoutSeconds = 3
            },
            Sources = new List<object>(),
            Exporters = new List<object>
            {
                new
                {
                    Type = "Fake",
                    SourceFilter = ".*"
                }
            }
        };

        for (var i = 1; i <= readersCount; i++)
        {
            settings.Sources.Add(new
            {
                Type = "Fake",
                Args = new FakeLogArgs
                {
                    Name = $"FakeSource{i}"
                }
            });
        }

        return settings;
    }
}