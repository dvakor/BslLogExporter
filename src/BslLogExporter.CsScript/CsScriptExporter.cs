using LogExporter.App.Exporters;
using LogExporter.App.Processing;
using Microsoft.Extensions.Logging;

namespace BslLogExporter.CsScript;

public sealed class CsScriptExporter : ILogExporter, IDisposable
{
    private readonly ILogger<CsScriptExporter> _logger;
    
    internal CsScriptExecutionContext ExecutionContext { get; }

    public CsScriptExporter(CsScriptExecutionContext executionContext, ILoggerFactory factory)
    {
        ExecutionContext = executionContext;
        _logger = factory.CreateLogger<CsScriptExporter>();
    }
    
    public async ValueTask ExportLogsAsync(SourceLogPortion portion)
    {
        var context = new CsScriptContext
        {
            Log = _logger,
            Args = ExecutionContext.ScriptArgs,
            Storage = ExecutionContext.Storage,
            Entries = portion.Entries,
            SourceName = portion.SourceName
        };
    }

    public void Dispose()
    {
        ExecutionContext.Dispose();
    }
}