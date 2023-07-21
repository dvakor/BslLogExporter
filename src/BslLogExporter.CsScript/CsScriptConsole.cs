using Dotnet.Script.Core;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace BslLogExporter.CsScript;

public class CsScriptConsole : ScriptConsole
{
    private readonly ILogger _logger;

    public CsScriptConsole(ILoggerFactory factory, string scriptPath) : base(
        TextWriter.Null, TextReader.Null, TextWriter.Null)
    {
        _logger = factory.CreateLogger($"CsScript.{new FileInfo(scriptPath).Name}");
    }

    public override string ReadLine()
    {
        _logger.LogWarning("Чтение данных с консоли не поддерживается");
        return string.Empty;
    }

    public override void WriteDiagnostics(Diagnostic[] warningDiagnostics, Diagnostic[] errorDiagnostics)
    {
        if (warningDiagnostics.Any())
        {
            var warnings = warningDiagnostics.Select(x => x.GetMessage()).ToList();
            _logger.LogWarning("Компиляция скрипта завершилась с предупреждениями: {Warnings}", warnings);
        }

        if (errorDiagnostics.Any())
        {
            var errors = errorDiagnostics.Select(x => x.GetMessage()).ToList();
            _logger.LogError("Компиляция скрипта завершилась с ошибками: {Errors}", errors);
        }
    }

    public override void WriteError(string value)
    {
        _logger.LogError("{ErrorMessage}", value);
    }

    public override void WriteSuccess(string value)
    {
        _logger.LogInformation("{SuccessMessage}", value);
    }

    public override void WriteWarning(string value)
    {
        _logger.LogWarning("{WarningMessage}", value);
    }

    public override void WriteNormal(string value)
    {
        _logger.LogInformation("{NormalMessage}", value);
    }

    public override void WriteHighlighted(string value)
    {
        _logger.LogInformation("{HighlightedMessage}", value);
    }

    public override void Clear()
    {
        // Ignore
    }
}