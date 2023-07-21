using LogExporter.Core.LogReader;

namespace LogExporter.App.Processing;

public class SourceLogPortion
{
    public string SourceName { get; init; }
    
    public IReadOnlyCollection<LogEntry> Entries { get; init; } 
}