using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace BslLogExporter.OScript;

public class OScriptAppHost : IHostApplication
{
    private readonly string[] _args;
    private readonly ILogger<OScriptAppHost> _logger;


    public OScriptAppHost(string[] args, ILoggerFactory loggerFactory)
    {
        _args = args;
        _logger = loggerFactory.CreateLogger<OScriptAppHost>();
    }
    
    public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
    {
        var level = status switch
        {
            MessageStatusEnum.WithoutStatus => LogLevel.Debug,
            MessageStatusEnum.Information => LogLevel.Information,
            MessageStatusEnum.Ordinary => LogLevel.Information,
            MessageStatusEnum.Attention => LogLevel.Warning,
            MessageStatusEnum.Important => LogLevel.Error,
            MessageStatusEnum.VeryImportant => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
        
        _logger.Log(level, "{ScriptMessage}", str);
    }

    public void ShowExceptionInfo(Exception exc)
    {
        _logger.LogError(exc, "Выведено исключение");
    }

    public bool InputString([UnscopedRef] out string result, string prompt, int maxLen, bool multiline)
    {
        result = string.Empty;
        return false;
    }

    public string[] GetCommandLineArguments() => _args;
}