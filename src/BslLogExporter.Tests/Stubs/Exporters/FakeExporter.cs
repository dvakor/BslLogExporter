using System.Collections.Generic;
using System.Threading.Tasks;
using LogExporter.App.Exporters;
using LogExporter.App.Processing;

namespace BslLogExporter.Tests.Stubs.Exporters;

public class FakeExporter : ILogExporter
{
    public List<SourceLogPortion> Portions { get; } = new();

    public Task ExportLogsAsync(SourceLogPortion portion)
    {
        Portions.Add(portion);
        
        return Task.CompletedTask;
    }
}