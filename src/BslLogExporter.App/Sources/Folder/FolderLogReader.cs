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
        _logFilesWatcher.DetectChanges();

        var fileFullPath = _logFilesWatcher.CurrentFiles.FirstOrDefault(x => x.EndsWith(file));

        if (!string.IsNullOrEmpty(fileFullPath))
        {
            ChangeFile(fileFullPath, position);
            return;
        }
        
        var nextFile = FindNextFileAfter(file);

        if (nextFile != null)
        {
            ChangeFile(nextFile, 0);
            return;
        }

        if (_logFilesWatcher.CurrentFiles.Length == 0)
        {
            throw new FileNotFoundException(file);
        }
        
        ChangeFile(_logFilesWatcher.CurrentFiles[0], 0);
    }

    private string? FindNextFileAfter(string fileName)
    {
        var targetFileName = Path.GetFileName(fileName);

        var sortedFiles = _logFilesWatcher.CurrentFiles
            .OrderBy(Path.GetFileName)
            .ToArray();

        return sortedFiles.FirstOrDefault(file 
            => string.Compare(Path.GetFileName(file), targetFileName, StringComparison.Ordinal) > 0);
    }

    public bool NextFile()
    {
        var directoryChanged = _logFilesWatcher.DetectChanges() != DirectoryChange.None;

        if (IsLastFile() && !directoryChanged)
        {
            return false;
        }

        var currentIndex = -1;

        if (_reader is not null)
        {
            currentIndex = _logFilesWatcher.CurrentFiles.IndexOf(_reader.FilePath);

            if (currentIndex == -1)
            {
                var currentFileName = Path.GetFileName(_reader.FilePath);
                var nextFile = FindNextFileAfter(currentFileName);

                if (nextFile != null)
                {
                    ChangeFile(nextFile, 0);
                    return true;
                }

                if (_logFilesWatcher.CurrentFiles.Length <= 0)
                {
                    return false;
                }
                
                ChangeFile(_logFilesWatcher.CurrentFiles[0], 0);
                return true;
            }
        }

        var lastIndex = _logFilesWatcher.CurrentFiles.LastIndex();

        if (currentIndex >= lastIndex)
        {
            return false;
        }

        ChangeFile(_logFilesWatcher.CurrentFiles[currentIndex + 1], 0);

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

        var currentIndex = _logFilesWatcher.CurrentFiles.IndexOf(_reader.FilePath);

        if (currentIndex == -1)
        {
            var currentFileName = Path.GetFileName(_reader.FilePath);
            var nextFile = FindNextFileAfter(currentFileName);
            return nextFile == null;
        }

        return currentIndex >= _logFilesWatcher.CurrentFiles.LastIndex();
    }

    public void Dispose()
    {
        _mdReader.Dispose();
        _reader?.Dispose();
    }
}