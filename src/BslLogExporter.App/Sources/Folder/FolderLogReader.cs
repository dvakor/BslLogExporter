using Ardalis.GuardClauses;
using LogExporter.Core.Extensions;
using LogExporter.Core.LogReader;
using LogExporter.Core.Metadata;
using LogExporter.Core.Watchers;

namespace LogExporter.App.Sources.Folder;

public class FolderLogReader
{
    private const string LogFileExt = "*.lgp";
    private const string MdFileName = "1Cv8.lgf";
    
    private readonly DirectoryWatcher _logFilesWatcher;
    private readonly MetadataReader _mdReader;

    private LogEntriesReader? _reader;

    public string? CurrentFilePath => _reader?.FilePath;

    public long? FileLength => _reader?.Length;

    public FolderLogReader(string folder)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException(folder);
        }
        
        _logFilesWatcher = new DirectoryWatcher(folder, LogFileExt);
        
        _mdReader = new MetadataReader(Path.Combine(folder, MdFileName));
        
        NextFile();
    }

    public BslLogEntry? GetNextEntry(CancellationToken token) => _reader?.GetNextEntry(token);

    public void ForwardTo(string file, long position)
    {
        var fileFullPath = _logFilesWatcher.CurrentFiles.FirstOrDefault(x => x.EndsWith(file));
        
        if (string.IsNullOrEmpty(fileFullPath))
        {
            throw new FileNotFoundException(file);
        }
        
        ChangeFile(fileFullPath, position);
    }

    public bool NextFile()
    {
        if (IsLastFile() 
            && _logFilesWatcher.DetectChanges() == DirectoryChange.None)
        {
            return false;
        }

        var currentIndex = -1;

        if (_reader is not null)
        {
            currentIndex = _logFilesWatcher.CurrentFiles.IndexOf(_reader?.FilePath);
        }

        var lastIndex = _logFilesWatcher.CurrentFiles.LastIndex();

        if (currentIndex >= lastIndex)
        {
            return false;
        }
        
        ChangeFile(_logFilesWatcher.CurrentFiles[currentIndex+1], 0);
            
        return true;

    }

    private void ChangeFile(string file, long position)
    {
        Guard.Against.NullOrWhiteSpace(file);

        var fileChanged = file != _reader?.FilePath;

        if (fileChanged)
        {
            _reader?.Dispose();
            _reader = new LogEntriesReader(file, _mdReader);
        }

        Guard.Against.Null(_reader);

        if (position > 0 && (position != _reader.CurrentPosition || fileChanged))
        {
            _reader.SetPosition(position);
        }
    }

    public bool IsLastFile()
    {
        if (_reader is null)
        {
            return _logFilesWatcher.CurrentFiles.Length == 0;
        }
        
        var currentIndex = _logFilesWatcher.CurrentFiles.IndexOf(_reader?.FilePath);

        return currentIndex >= _logFilesWatcher.CurrentFiles.LastIndex();
    }

    public void Dispose()
    {
        _mdReader.Dispose();
        _reader?.Dispose();
    }
}