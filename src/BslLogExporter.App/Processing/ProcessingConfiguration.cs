namespace LogExporter.App.Processing;

public record ProcessingConfiguration
{
    public int BufferSize { get; init; } = 5000;

    public int BufferTimeoutSeconds { get; init; } = 5;
}