using System.Diagnostics;
using LogExporter.Core.Extensions;

namespace LogExporter.Core.Watchers
{
    public class DirectoryWatcher
    {
        private const int PollInterval = 1000;
        
        private readonly string _filePattern;
        private readonly DirectoryInfo _directoryInfo;
        private readonly Stopwatch _stopwatch = new();

        public string[] CurrentFiles { get; private set; }

        public DirectoryWatcher(string directoryPath, string filePattern)
        {
            _stopwatch.Start();
            _filePattern = filePattern;
            _directoryInfo = new DirectoryInfo(directoryPath);
            CurrentFiles = GetFiles();
        }

        public DirectoryChange DetectChanges()
        {
            if (_stopwatch.ElapsedMilliseconds < PollInterval)
            {
                return DirectoryChange.None;
            }
            
            _stopwatch.Restart();
            
            var newFiles = GetFiles();

            if (!newFiles.DiffersFrom(CurrentFiles))
            {
                return DirectoryChange.None;
            }
            
            CurrentFiles = newFiles;
            return DirectoryChange.Changed;
        }

        private string[] GetFiles()
        {
            try
            {
                _directoryInfo.Refresh();
            }
            catch
            {
                //NoOp
            }
            
            if (!_directoryInfo.Exists)
            {
                return Array.Empty<string>();
            }
            
            return _directoryInfo
                .EnumerateFiles(_filePattern, SearchOption.TopDirectoryOnly)
                .OrderBy(x => x.Name)
                .Select(x => x.FullName)
                .ToArray();
        }
    }
}