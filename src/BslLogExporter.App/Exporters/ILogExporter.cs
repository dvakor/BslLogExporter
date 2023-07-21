using LogExporter.App.Processing;

namespace LogExporter.App.Exporters;

public interface ILogExporter
{
    Task ExportLogsAsync(SourceLogPortion portion);
}