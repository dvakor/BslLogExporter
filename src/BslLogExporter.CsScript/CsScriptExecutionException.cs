namespace BslLogExporter.CsScript;

public class CsScriptExecutionException : Exception
{
    public CsScriptExecutionException(int code) : base($"Выполнение скрипта завершилось с кодом ошибки: {code}")
    {
        
    }
}