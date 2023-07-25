using LogExporter.App.History;

namespace BslLogExporter.Tests.Stubs.History;

public class InMemoryLogHistoryStorage : ILogHistoryStorage
{
    public PositionsData Data { get; } = new();
    
    public ValueTask SaveHistoryAsync(string sourceName, string fileName, long position)
    {
        Data.Add(sourceName, fileName, position);
        return ValueTask.CompletedTask;
    }

    public ValueTask<HistoryRecord> GetLastPositionAsync(string sourceName)
    {
        return ValueTask.FromResult(Data.GetLastPosition(sourceName));
    }

    public class PositionsData
    {
        public Dictionary<string, HistoryRecord> Positions { get; private set; } = new();

        public void Add(string sourceName, string fileName, long filePosition)
        {
            var record = new HistoryRecord
            {
                File = fileName,
                Position = filePosition
            };

            Positions[sourceName] = record;
        }

        public HistoryRecord GetLastPosition(string sourceFile)
        {
            return Positions.TryGetValue(sourceFile, out var record) 
                ? record 
                : HistoryRecord.Empty;
        }
    }
}