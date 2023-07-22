using System.Runtime;

namespace BslLogExporter.CsScript;

public class CsScriptMethodSearchException : Exception
{
    private const string OnLogExporting = "OnLogExporting";
    
    private CsScriptMethodSearchException(string message) : base(message)
    {
        
    }

    public static CsScriptMethodSearchException NotFound(string scriptPath)
    {
        var message = $"Скрипт {scriptPath} не содержит метода {OnLogExporting}";
        return new CsScriptMethodSearchException(message);
    }
    
    public static CsScriptMethodSearchException AmbiguousMethods(string scriptPath, IEnumerable<Type> types)
    {
        var names = types.Select(x => x.Name);
        var stringNames = string.Join(',', names);
        var message = $"Скрипт {scriptPath} содержит более 1 типа с методом {OnLogExporting}: {stringNames}";
        return new CsScriptMethodSearchException(message);
    }

    public static CsScriptMethodSearchException InvalidMethodSignature(string scriptPath)
    {
        var message = $"{OnLogExporting} скрипта {scriptPath} является не корректным";
        return new CsScriptMethodSearchException(message);
    }
}