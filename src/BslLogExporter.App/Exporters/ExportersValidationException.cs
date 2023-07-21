namespace LogExporter.App.Exporters;

public class ExportersValidationException : Exception
{
    public ExportersValidationException(Exception inner) : base("При валидации экспортеров произошла ошибка", inner)
    {
        
    }
}