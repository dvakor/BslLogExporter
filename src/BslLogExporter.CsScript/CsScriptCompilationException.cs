using Microsoft.CodeAnalysis;

namespace BslLogExporter.CsScript;

public class CsScriptCompilationException : Exception
{
    public CsScriptCompilationException(IEnumerable<Diagnostic> compilationErrors) : base(GetMessage(compilationErrors))
    {
        
    }

    private static string GetMessage(IEnumerable<Diagnostic> compilationErrors)
    {
        var errors = string.Join(Environment.NewLine, compilationErrors.Select(x 
            => $"{HumanizeLocation(x.Location)}{Environment.NewLine}{x.GetMessage()}"));
        return $"Компиляция скрипта завершилась с ошибками: {errors}";
    }

    private static string HumanizeLocation(Location location)
    {
        if (location.Kind == LocationKind.SourceFile)
        {
            var tree = location.SourceTree!;

            var spanLocation = tree.GetLineSpan(location.SourceSpan);

            var startLine = spanLocation.StartLinePosition.Line + 1;
            var startChar = spanLocation.StartLinePosition.Character + 1;
            
            var endLine = spanLocation.EndLinePosition.Line + 1;
            var endChar = spanLocation.EndLinePosition.Character + 1;

            if (startLine != endLine || startChar != endChar)
            {
                return $"{tree.FilePath} ({startLine}:{startChar} - {endLine}:{endChar})";
            }

            return $"{tree.FilePath} ({startLine}:{startChar})";
        }

        return location.ToString();
    }
}