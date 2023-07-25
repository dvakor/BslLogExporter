using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Exporters;

public sealed class LogExportersSnapshot : IDisposable
{
    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

    private readonly ConcurrentDictionary<string, IReadOnlyCollection<ILogExporter>> _sourcesExporters = new();

    public IChangeToken ChangeToken { get; init; } = default!;
    
    public IReadOnlyList<KeyValuePair<string, ILogExporter>> Exporters { get; init; } = default!;

    public IReadOnlyCollection<ILogExporter> GetExportersFor(string source)
    {
        return _sourcesExporters.GetOrAdd(source, srcName =>
        {
            return Exporters
                .Where(x => Regex.IsMatch(srcName, x.Key, Options))
                .Select(x => x.Value)
                .ToList();
        });
    }

    public void Dispose()
    {
        var disposables = Exporters
            .Select(x => x.Value)
            .OfType<IDisposable>();

        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}