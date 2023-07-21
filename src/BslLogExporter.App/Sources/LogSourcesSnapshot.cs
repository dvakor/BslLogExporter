using Microsoft.Extensions.Primitives;

namespace LogExporter.App.Sources;

public sealed class LogSourcesSnapshot : IDisposable
{
    public IChangeToken ChangeToken { get; init; }

    public IReadOnlyCollection<ILogSource> Sources { get; init; }

    public void Dispose()
    {
        var disposables = Sources.OfType<IDisposable>();
        
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}