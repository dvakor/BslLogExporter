using System.Reflection;
using CSScripting;

namespace BslLogExporter.CsScript;

public sealed class CsScriptExecutionContext : IDisposable
{
    private readonly string _scriptPath;
    private readonly Assembly _assembly;
    
    internal CsScriptExecutionContext(string scriptPath, Assembly assembly, string[] scriptArgs)
    {
        _scriptPath = scriptPath;
        _assembly = assembly;
        ScriptArgs = scriptArgs;
    }
    
    public string[] ScriptArgs { get; }
    
    public CsScriptStorage Storage { get; } = new();

    public void Dispose()
    {
        _assembly.Unload();
        Storage.Dispose();
    }

    private Func<CsScriptContext, ValueTask> FindMethod()
    {
        var candidateTypes = _assembly
            .GetExportedTypes()
            .Select(x => new
            {
                Type = x,
                Methods = x.GetMethods()
                    .Where(m => m.Name.Equals("OnLogExporting", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            })
            .Where(x => x.Methods.Any())
            .ToList();

        switch (candidateTypes.Count)
        {
            case 0:
                throw CsScriptMethodSearchException.NotFound(_scriptPath);
            case > 1:
                throw CsScriptMethodSearchException.AmbiguousMethods(_scriptPath, candidateTypes.Select(x => x.Type));
        }

        var candidate = candidateTypes.First();

        if (candidate.Methods.Count > 1)
        {
            throw CsScriptMethodSearchException.AmbiguousMethods(_scriptPath, candidateTypes.Select(x => x.Type));
        }

        var method = candidate.Methods.First();

        var methodParameters = method.GetParameters();
        
        if (methodParameters.Length != 1)
        {
            throw CsScriptMethodSearchException.InvalidMethodSignature(_scriptPath);
        }

        var isSync = method.ReturnType == typeof(void);

        throw new NotImplementedException();
    }
}