using Ardalis.GuardClauses;
using LogExporter.App.Exporters;
using Microsoft.Extensions.Logging;

namespace BslLogExporter.CsScript;

public class CsScriptExporterFactory : AbstractExporterFactory<CSharpScriptArgs>
{
    private readonly CsScriptCompiler _compiler;
    private readonly ILoggerFactory _factory;

    public override string TypeName => "CsScript";

    public CsScriptExporterFactory(CsScriptCompiler compiler, ILoggerFactory factory)
    {
        _compiler = compiler;
        _factory = factory;
    }

    protected override ILogExporter CreateExporter(CSharpScriptArgs args)
    {
        var context = CreateExecutionContext(args);

        return new CsScriptExporter(context, _factory);
    }

    private CsScriptExecutionContext CreateExecutionContext(CSharpScriptArgs args)
    {
        Guard.Against.NullOrWhiteSpace(args.PathToScript);

        var fi = new FileInfo(args.PathToScript);

        if (!fi.Exists)
        {
            throw new FileNotFoundException(fi.FullName);
        }
        
        var scriptArgs = args.ScriptArgs ?? Array.Empty<string>();
        
        var scriptInstance = _compiler.CompileScript(fi.FullName);

        var context = new CsScriptExecutionContext(fi.FullName, scriptInstance, scriptArgs);

        return context;
    }
}