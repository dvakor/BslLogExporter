using LogExporter.App.History;
using LogExporter.Core.LogReader;

namespace LogExporter.App.Sources.Folder;

public sealed class FolderLogSource : ILogSource, IDisposable
{
    private readonly bool _liveMode;
    private readonly FolderLogReader _reader;

    public string Name { get; }

    public FolderLogSource(string name, string folder, bool liveMode)
    {
        _liveMode = liveMode;
        _reader = new FolderLogReader(folder);
        
        Name = name;
    }
    
    public IEnumerable<BslLogEntry> GetLogs(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var entry = _reader.GetNextEntry(token);

            if (entry is null && !_reader.NextFile())
            {
                yield break;
            }

            while (entry is not null)
            {
                yield return entry;
                entry = _reader.GetNextEntry(token);
            }
        }
    }

    public void ForwardTo(HistoryRecord record)
    {
        if (record.IsEmpty())
        {
            if (_liveMode)
            {
                EnterLiveMode();
            }
            return;
        }

        _reader.ForwardTo(record.File!, record.Position);
    }
    
    public void Dispose()
    {
        _reader.Dispose();
    }
    
    private void EnterLiveMode()
    {
        while (!_reader.IsLastFile())
        {
            _reader.NextFile();
        }

        if (!string.IsNullOrWhiteSpace(_reader.CurrentFilePath))
        {
            _reader.ForwardTo(_reader.CurrentFilePath, _reader.FileLength!.Value);
        }
    }
}