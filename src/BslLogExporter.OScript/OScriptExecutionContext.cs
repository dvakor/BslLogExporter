using ScriptEngine.HostedScript.Library;

namespace BslLogExporter.OScript;

public sealed class OScriptExecutionContext : IDisposable
{
    public SystemGlobalContext GlobalContext { get; }
    
    public OScriptInstance Instance { get; }
    
    internal OScriptExecutionContext(SystemGlobalContext globalContext, OScriptInstance instance)
    {
        GlobalContext = globalContext;
        Instance = instance;
    }

    public void Dispose()
    {
        GlobalContext.DisposeObject(Instance);
        GlobalContext.EngineInstance.Dispose();
    }
}