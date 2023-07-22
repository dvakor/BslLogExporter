using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Dotnet.Script.Core;
using Dotnet.Script.DependencyModel.Environment;
using Dotnet.Script.DependencyModel.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BslLogExporter.CsScript;

public partial class CsScriptCompiler : ScriptCompiler
{
    private readonly List<string> _importedNamespaces;
    private readonly ILogger<CsScriptCompiler> _compilerLogger;

    protected override IEnumerable<string> ImportedNamespaces => _importedNamespaces;

    public CsScriptCompiler(IOptions<CsScriptSettings> settings, ILoggerFactory factory) : base(factory.CreateCsLogger, true)
    {
        _compilerLogger = factory.CreateLogger<CsScriptCompiler>();
        
        AssemblyLoadContext = AssemblyLoadContext.Default;
        
        _importedNamespaces = base.ImportedNamespaces.ToList();
        _importedNamespaces.Add("Microsoft.Extensions.Logging");
        
        InitializeScriptEnvironment(settings.Value);
    }

    public override ScriptOptions CreateScriptOptions(ScriptContext context, IList<RuntimeDependency> runtimeDependencies)
    {
        var options = base.CreateScriptOptions(context, runtimeDependencies);
        return options
            .AddReferences("Microsoft.Extensions.Logging")
            .AddReferences("BslLogExporter.CsScript")
            .AddReferences("BslLogExporter.Core");
    }

    public async Task<CsScriptExecutionContext> CreateExecutionContextAsync(string pathToFile, string[] args)
    {
        var fi = new FileInfo(pathToFile);

        if (!fi.Exists)
        {
            throw new FileNotFoundException(fi.FullName);
        }

        var rootedPath = fi.FullName;
        
        var text = await File.ReadAllTextAsync(rootedPath);
        var code = SourceText.From(text);
        var script = new ScriptContext(code, Environment.CurrentDirectory, args,
            rootedPath, OptimizationLevel.Release);

        var compilation = CreateCompilationContext<int, CsScriptHost>(script);

        if (compilation.Warnings.Any())
        {
            var warnings = compilation.Warnings.Select(x => x.GetMessage()).ToList();
            _compilerLogger.LogWarning("Компиляция скрипта завершилась с предупреждениями: {Warnings}", warnings);
        }

        if (compilation.Errors.Any())
        {
            throw new CsScriptCompilationException(compilation.Errors);
        }
        
        var context = new CsScriptExecutionContext(script, compilation);

        return context;
    }
    
    private static void InitializeScriptEnvironment(CsScriptSettings settingsValue)
    {
        var netVersion = settingsValue.TargetFramework;

        if (string.IsNullOrEmpty(netVersion))
        {
            netVersion = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
        }
        
        ScriptEnvironment.Default.OverrideTargetFramework(netVersion);

        var scriptCachesLocation = settingsValue.CacheDirectory;

        scriptCachesLocation = string.IsNullOrEmpty(scriptCachesLocation) 
            ? Path.Combine(Environment.CurrentDirectory, "csx_cache") 
            : WorkingDirRegex().Replace(scriptCachesLocation, Environment.CurrentDirectory);

        Environment.SetEnvironmentVariable("DOTNET_SCRIPT_CACHE_LOCATION", scriptCachesLocation);
    }

    [GeneratedRegex("{WorkingDir}")]
    private static partial Regex WorkingDirRegex();
}