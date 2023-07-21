using Dotnet.Script.Core;

namespace BslLogExporter.CsScript;

public sealed class CsScriptExecutionContext : IDisposable
{
    internal CsScriptExecutionContext(ScriptContext script, ScriptCompilationContext<int> compilation)
    {
        Script = script;
        Compilation = compilation;
    }

    public ScriptContext Script { get; }
    
    public ScriptCompilationContext<int> Compilation { get; }

    public CsScriptStorage Storage { get; } = new();

    public void Dispose()
    {
        Storage.Dispose();
    }
}