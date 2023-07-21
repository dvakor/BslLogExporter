using LogExporter.App.Exporters;
using LogExporter.App.Processing;

namespace BslLogExporter.CsScript;

public sealed class CsScriptExporter : ILogExporter, IDisposable
{
    private readonly string _scriptPath;
    private readonly string[] _scriptArgs;
    private readonly CsScriptCompiler _compiler;
    private readonly CsScriptExecutor _executor;
    
    internal CsScriptExecutionContext? Context { get; private set; }

    public CsScriptExporter(
        string scriptPath, string[] scriptArgs, 
        CsScriptCompiler compiler, CsScriptExecutor executor)
    {
        _scriptPath = scriptPath;
        _scriptArgs = scriptArgs;
        _compiler = compiler;
        _executor = executor;
    }
    
    public async Task ExportLogsAsync(SourceLogPortion portion)
    {
        Context ??= await _compiler.CreateExecutionContextAsync(_scriptPath, _scriptArgs);

        await _executor.ExecuteAsync(Context, new CsScriptContext
        {
            Entries = portion.Entries,
            SourceName = portion.SourceName
        });
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}