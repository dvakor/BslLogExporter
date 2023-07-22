using LogExporter.Core.LogReader;
using Microsoft.Extensions.Logging;

namespace BslLogExporter.CsScript;

public sealed class CsScriptContext
{
    public string[] Args { get; init; } = default!;

    public ILogger Log { get; init; } = default!;
    
    public CsScriptStorage Storage { get; init; } = default!;
    
    public IEnumerable<LogEntry> Entries { get; init; } = default!;
    
    public string SourceName { get; init; } = default!;
}