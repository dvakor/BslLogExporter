using System.Reflection;
using System.Text.RegularExpressions;
using CSScriptLib;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BslLogExporter.CsScript;

public partial class CsScriptCompiler
{
    [GeneratedRegex("{WorkingDir}")]
    private static partial Regex WorkingDirRegex();
    
    [GeneratedRegex(@"\/|\.|\\")]
    private static partial Regex PathToNameRegex();
    
    private readonly RoslynEvaluator _evaluator;
    private readonly EvaluatorConfig _evaluatorConfig;

    public CsScriptCompiler(IOptions<CsScriptSettings> settings)
    {
        _evaluator = CSScript.RoslynEvaluator;
        _evaluatorConfig = CSScript.EvaluatorConfig;
        InitializeScriptEngineEnvironment(settings.Value);
    }

    public Assembly CompileScript(string scriptPath)
    {
        var asmName = PathToNameRegex().Replace(scriptPath, "_") + ".dll";
        
        var asmPath = _evaluator
            .CompileAssemblyFromFile(scriptPath, new CompileInfo
            {
                AssemblyFile = asmName
            });
        
        return Assembly.LoadFile(asmPath);
    }

    private void InitializeScriptEngineEnvironment(CsScriptSettings settingsValue)
    {
        var scriptCachesLocation = settingsValue.CacheDirectory;
        
        scriptCachesLocation = string.IsNullOrEmpty(scriptCachesLocation) 
            ? Path.Combine(Environment.CurrentDirectory, "scripts_cache") 
            : WorkingDirRegex().Replace(scriptCachesLocation, Environment.CurrentDirectory);
        
        Environment.SetEnvironmentVariable("CSS_CUSTOM_TEMPDIR", scriptCachesLocation);

        if (!Directory.Exists(scriptCachesLocation))
        {
            Directory.CreateDirectory(scriptCachesLocation);
        }
        
        _evaluatorConfig.Engine = EvaluatorEngine.Roslyn;
        _evaluatorConfig.PdbFormat = DebugInformationFormat.PortablePdb;

        _evaluator
            .ReferenceAssemblyOf<CsScriptCompiler>()
            .ReferenceAssemblyOf<string>()
            .ReferenceAssemblyOf<IQueryable>()
            .ReferenceAssemblyOf<ILogger>()
            .ReferenceAssemblyOf(typeof(List<>));
        
        _evaluator.DebugBuild = false;
        _evaluator.IsCachingEnabled = true;
        _evaluator.IsAssemblyUnloadingEnabled = true;
    }
}