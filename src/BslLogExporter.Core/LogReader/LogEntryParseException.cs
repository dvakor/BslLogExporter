using LogExporter.Core.Brackets.Values;

namespace LogExporter.Core.LogReader
{
    public class LogEntryParseException : Exception
    {
        public LogEntryParseException(BracketsNodeValue node, string fileName, Exception innerException) 
            : base($"Ошибка парсинга элемента: {node}, в файле {fileName}", innerException)
        {
            
        }
    }
}