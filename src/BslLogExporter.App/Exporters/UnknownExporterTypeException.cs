namespace LogExporter.App.Exporters;

public class UnknownExporterTypeException : Exception
{
    public UnknownExporterTypeException(string type) : base($"Неизвестный тип экспортера: {type}")
    {
        
    }
}