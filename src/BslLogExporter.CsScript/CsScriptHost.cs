using Microsoft.Extensions.Logging;

namespace BslLogExporter.CsScript;

public class CsScriptHost
{
    public IReadOnlyCollection<string> Args { get; init; } = default!;
    
    public CsScriptContext Context { get; init; } = default!;
    
    public ILogger Log { get; init; } = default!;
    
    public CsScriptStorage Storage { get; init; } = default!;
}