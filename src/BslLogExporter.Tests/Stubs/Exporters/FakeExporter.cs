using LogExporter.App.Exporters;
using LogExporter.App.Processing;

namespace BslLogExporter.Tests.Stubs.Exporters;

public class FakeExporter : ILogExporter
{
    public List<SourceLogPortion> Portions { get; } = new();

    public ValueTask ExportLogsAsync(SourceLogPortion portion)
    {
        Portions.Add(portion);
        
        return ValueTask.CompletedTask;
    }
}