using Microsoft.Extensions.Logging;
using ScriptEngine.Machine.Contexts;

namespace BslLogExporter.OScript;

[ContextClass("ЛоггерСкрипта")]
public class OScriptLogWrapper : AutoContext<OScriptLogWrapper>
{
    private readonly ILogger<OScriptLogWrapper> _logger;

    public OScriptLogWrapper(ILoggerFactory factory)
    {
        _logger = factory.CreateLogger<OScriptLogWrapper>();
    }

    [ContextMethod("Информация")]
    public void Information(string message)
    {
        _logger.LogInformation("{InfoMessage}", message);
    }
    
    [ContextMethod("Отладка")]
    public void Debug(string message)
    {
        _logger.LogDebug("{DebugMessage}", message);
    }
    
    [ContextMethod("Предупреждение")]
    public void Warning(string message)
    {
        _logger.LogWarning("{WarnMessage}", message);
    }
    
    [ContextMethod("Ошибка")]
    public void Error(string message)
    {
        _logger.LogError("{ErrMessage}", message);
    }
}