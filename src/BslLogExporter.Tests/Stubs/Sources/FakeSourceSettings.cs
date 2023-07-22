using System;
using System.Collections.Generic;
using System.Threading;
using LogExporter.Core.LogReader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace BslLogExporter.Tests.Stubs.Sources;

public class FakeSourceSettings
{
    public Func<string, CancellationToken, IEnumerable<BslLogEntry>> LogsProvider { get; set; } =
        (_, _) => Array.Empty<BslLogEntry>();
    
    public Func<IChangeToken> ChangeToken { get; set; } = () => NullChangeToken.Singleton;
}