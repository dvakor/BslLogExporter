using System.Threading.Tasks;
using BslLogExporter.OScript;
using BslLogExporter.Tests.Stubs;
using LogExporter.App.Processing;
using LogExporter.Core.LogReader;
using Microsoft.Extensions.Options;
using Moq;
using ScriptEngine.Machine;
using Xunit.Abstractions;

namespace BslLogExporter.Tests;

public class OScriptExporterTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private IOptions<OScriptSettings> _settings;

    public OScriptExporterTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        var mock = new Mock<IOptions<OScriptSettings>>();
        mock
            .SetupGet(x => x.Value)
            .Returns(() => new OScriptSettings());

        _settings = mock.Object;
    }
    
    [Fact]
    public async Task Should_Compile_And_Run_Script()
    {
        var factory = TestOutputLoggerFactory.Create(_testOutputHelper);
        
        var exporter = SutFactory.CreateExporter(new OScriptArgs
            {
                PathToScript = "./Files/TestScript.os",
                ScriptArgs = new [] {"Hello"}
            }, 
            () => new OScriptExporterFactory(_settings, factory));

        var record = await Record.ExceptionAsync(() =>
        {
            return exporter.ExportLogsAsync(new SourceLogPortion
            {
                SourceName = "TestSource",
                Entries = new BslLogEntry[] { 
                    new()
                    {
                        FileName = "Generated",
                        Position = 1
                    }
                }
            });
        });
        
        Assert.Null(record);
    }

    [Fact]
    public async Task Should_Export_Logs_Via_OScript()
    {
        var factory = TestOutputLoggerFactory.Create(_testOutputHelper);
        
        var exporter = (OScriptExporter)SutFactory.CreateExporter(new OScriptArgs
            {
                PathToScript = "./Files/TestExporter.os",
                ScriptArgs = new [] {"Hello"}
            }, 
            () => new OScriptExporterFactory(_settings, factory));

        var portion = new SourceLogPortion
        {
            SourceName = "TestSource",
            Entries = new BslLogEntry[]
            {
                new()
                {
                    FileName = "Generated",
                    Position = 1
                }
            }
        };

        await exporter.ExportLogsAsync(portion);
        
        await exporter.ExportLogsAsync(portion);

        var osValue = exporter.ExecutionContext.Instance.Storage
            .GetIndexedValue(ValueFactory.Create("Записи"));

        var totalLogs = osValue.AsNumber();
        
        Assert.StrictEqual(2, totalLogs);
    }
}