using LogExporter.Core.LogReader;

namespace BslLogExporter.CsScript;

public sealed class CsScriptContext
{
    public IEnumerable<BslLogEntry> Entries { get; init; } = default!;
    
    public string SourceName { get; init; } = default!;
}