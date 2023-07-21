namespace LogExporter.App.Sources;

public class SourcesValidationException : Exception
{
    public SourcesValidationException(Exception inner) : base("При валидации источников произошла ошибка", inner)
    {
        
    }
}