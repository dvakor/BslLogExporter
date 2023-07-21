using Ardalis.GuardClauses;
using LogExporter.App.Exporters;

namespace BslLogExporter.CsScript;

public class CsScriptExporterFactory : AbstractExporterFactory<CSharpScriptArgs>
{
    private readonly CsScriptCompiler _compiler;
    private readonly CsScriptExecutor _executor;
    
    public override string TypeName => "CsScript";

    public CsScriptExporterFactory(CsScriptCompiler compiler, CsScriptExecutor executor)
    {
        _compiler = compiler;
        _executor = executor;
    }

    protected override ILogExporter CreateExporter(CSharpScriptArgs args)
    {
        Guard.Against.NullOrWhiteSpace(args.PathToScript);

        return new CsScriptExporter(
            args.PathToScript, args.ScriptArgs ?? Array.Empty<string>(), 
            _compiler, _executor);
    }
}