using BslLogExporter.CsScript;
using BslLogExporter.Tests.Stubs;
using LogExporter.App.Processing;
using LogExporter.Core.LogReader;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;

namespace BslLogExporter.Tests;

public class CsScriptExporterTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private IOptions<CsScriptSettings> _settings;

    public CsScriptExporterTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var mock = new Mock<IOptions<CsScriptSettings>>();
        mock
            .SetupGet(x => x.Value)
            .Returns(() => new CsScriptSettings());

        _settings = mock.Object;
    }
    
    [Theory]
    [InlineData("TestExporterSync.cs")]
    [InlineData("TestExporterAsync.cs")]
    public async Task Should_Compile_And_Run_Script(string scriptName)
    {
        var factory = TestOutputLoggerFactory.Create(_testOutputHelper);

        var compiler = new CsScriptCompiler(_settings);

        var exporter = SutFactory.CreateExporter(new CSharpScriptArgs
            {
                PathToScript = $"./Files/{scriptName}",
                ScriptArgs = new [] {"Hello"}
            }, 
            () => new CsScriptExporterFactory(compiler, factory));

        var record = await Record.ExceptionAsync(async () =>
        {
            await exporter.ExportLogsAsync(new SourceLogPortion
            {
                SourceName = "TestSource",
                Entries = new LogEntry[] { 
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
    public async Task Should_Export_Logs_Via_CsScript()
    {
        var factory = TestOutputLoggerFactory.Create(_testOutputHelper);

        var compiler = new CsScriptCompiler(_settings);
        
        var exporter = (CsScriptExporter)SutFactory.CreateExporter(new CSharpScriptArgs
            {
                PathToScript = "./Files/TestExporter.csx"
            }, 
            () => new CsScriptExporterFactory(compiler, factory));

        var portion = new SourceLogPortion
        {
            SourceName = "TestSource",
            Entries = new LogEntry[]
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

        var totalLogs = exporter.ExecutionContext.Storage.Get<int>("Count");
        
        Assert.StrictEqual(2, totalLogs);
    }
}