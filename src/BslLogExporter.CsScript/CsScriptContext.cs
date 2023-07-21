using LogExporter.Core.LogReader;

namespace BslLogExporter.CsScript;

public sealed class CsScriptContext
{
    public IEnumerable<LogEntry> Entries { get; init; } = default!;
    
    public string SourceName { get; init; } = default!;
}