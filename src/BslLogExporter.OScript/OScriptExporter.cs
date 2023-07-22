using LogExporter.App.Exporters;
using LogExporter.App.Processing;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace BslLogExporter.OScript;

public sealed class OScriptExporter : ILogExporter, IDisposable
{
    internal OScriptExecutionContext ExecutionContext { get; }

    public OScriptExporter(OScriptExecutionContext context)
    {
        ExecutionContext = context;
    }

    public ValueTask ExportLogsAsync(SourceLogPortion portion)
    {
        var osEntries = portion.Entries.Select(x => new OScriptLogEntry(x));
        var osArray = new ArrayImpl(osEntries);
        
        ExecutionContext.Instance.Context = new OScriptContext
        {
            SourceName = ValueFactory.Create(portion.SourceName),
            Entries = new FixedArrayImpl(osArray)
        };
        
        ExecutionContext.GlobalContext.EngineInstance.UpdateContexts();
        ExecutionContext.GlobalContext.EngineInstance.InitializeSDO(ExecutionContext.Instance);

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        ExecutionContext.Dispose();
    }
}