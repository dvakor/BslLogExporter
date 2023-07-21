using LogExporter.Core.LogReader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace BslLogExporter.Tests.Stubs.Sources;

public class FakeSourceSettings
{
    public Func<string, CancellationToken, IEnumerable<LogEntry>> LogsProvider { get; set; } =
        (_, _) => Array.Empty<LogEntry>();
    
    public Func<IChangeToken> ChangeToken { get; set; } = () => NullChangeToken.Singleton;
}