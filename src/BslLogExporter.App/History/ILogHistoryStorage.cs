namespace LogExporter.App.History;

public interface ILogHistoryStorage
{
    ValueTask SaveHistoryAsync(string sourceName, string fileName, long position);

    ValueTask<HistoryRecord> GetLastPositionAsync(string sourceName);
}