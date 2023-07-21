using Dotnet.Script.Core;
using Microsoft.Extensions.Logging;

namespace BslLogExporter.CsScript;

public class CsScriptExecutor
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ScriptCompiler _compiler;

    public CsScriptExecutor(
        CsScriptCompiler compiler,
        ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _compiler = compiler;
    }

    public async Task ExecuteAsync(CsScriptExecutionContext executionContext, CsScriptContext scriptContext)
    {
        var runner = new ScriptRunner(_compiler, _loggerFactory.CreateCsLogger,
            new CsScriptConsole(_loggerFactory, executionContext.Script.FilePath));

        var code = await runner.Execute(executionContext.Compilation, new CsScriptHost
        {
            Args = executionContext.Script.Args,
            Context = scriptContext,
            Log = _loggerFactory.CreateLogger(ScriptCategoryName(executionContext)),
            Storage = executionContext.Storage
        });
        
        if (code != 0)
        {
            throw new CsScriptExecutionException(code);
        }
    }

    private static string ScriptCategoryName(CsScriptExecutionContext executionContext)
    {
        return $"{typeof(CsScriptExecutor).FullName}.{new FileInfo(executionContext.Script.FilePath).Name}";
    }
}