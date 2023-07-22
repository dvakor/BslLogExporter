using System.Reflection;
using Ardalis.GuardClauses;
using LogExporter.App.Exporters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace BslLogExporter.OScript;

public class OScriptExporterFactory : AbstractExporterFactory<OScriptArgs>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly OScriptSettings _settings;

    public override string TypeName => "OneScript";

    public OScriptExporterFactory(IOptions<OScriptSettings> settings, ILoggerFactory loggerFactory)
    {
        _settings = settings.Value;
        _loggerFactory = loggerFactory;
    }
    
    protected override ILogExporter CreateExporter(OScriptArgs args)
    {
        var context = CreateExecutionContext(args);
        return new OScriptExporter(context);
    }
    
    private OScriptExecutionContext CreateExecutionContext(OScriptArgs args)
    {
        Guard.Against.NullOrWhiteSpace(args.PathToScript);
        
        var fi = new FileInfo(args.PathToScript);

        if (!fi.Exists)
        {
            throw new FileNotFoundException(fi.FullName);
        }

        var scriptArgs = args.ScriptArgs ?? Array.Empty<string>();

        var engine = CreateEngine();
        var host = new OScriptAppHost(scriptArgs, _loggerFactory);
        var source = new OScriptCodeSource(args.PathToScript);

        var globalContext = new SystemGlobalContext
        {
            EngineInstance = engine,
            ApplicationHost = host,
            CodeSource = source
        };

        engine.Environment.InjectObject(globalContext);
        GlobalsManager.RegisterInstance(globalContext);

        engine.UpdateContexts();

        var compiler = engine.GetCompilerService();

        var instance = new OScriptInstance(compiler, source, _loggerFactory);

        return new OScriptExecutionContext(globalContext, instance);
    }

    private ScriptingEngine CreateEngine()
    {
        var engine = new ScriptingEngine();
        engine.Environment = new RuntimeEnvironment();

        engine.AttachAssembly(Assembly.GetExecutingAssembly(), engine.Environment);
        engine.AttachAssembly(typeof(SystemGlobalContext).Assembly, engine.Environment);
        engine.Initialize();

        if (string.IsNullOrWhiteSpace(_settings.LibraryDir))
        {
            return engine;
        }
        
        var libResolver = new LibraryResolver(engine, engine.Environment)
        {
            LibraryRoot = _settings.LibraryDir
        };
            
        engine.DirectiveResolvers.Add(libResolver);

        return engine;
    }
}