using LogExporter.App.History;
using LogExporter.Core.LogReader;

namespace LogExporter.App.Sources;

public interface ILogSource
{ 
    string Name { get; }
    
    IEnumerable<BslLogEntry> GetLogs(CancellationToken token);
    
    void ForwardTo(HistoryRecord record);
}