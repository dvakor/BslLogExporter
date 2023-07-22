using LogExporter.App.Processing;

namespace LogExporter.App.Exporters;

public interface ILogExporter
{
    ValueTask ExportLogsAsync(SourceLogPortion portion);
}