namespace LogExporter.App.History;

public record HistoryRecord
{
    public string? File { get; init; }
    
    public long Position { get; init; }

    public bool IsEmpty() => this == Empty;

    public static HistoryRecord Empty { get; } = new();
}