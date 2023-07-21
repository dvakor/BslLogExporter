using System.Diagnostics;

namespace LogExporter.Core.Watchers
{
    public class FileWatcher
    {
        private const int PollInterval = 1000;

        private readonly Stopwatch _stopwatch = new();
        private readonly FileInfo _fileInfo;

        private bool _exist;
        private long _lenght;
        
        public FileWatcher(string file)
        {
            _stopwatch.Start();
            _fileInfo = new FileInfo(file);
            _exist = _fileInfo.Exists;
            _lenght = _fileInfo.Length;
        }

        public FileChange DetectChanges()
        {
            if (_stopwatch.ElapsedMilliseconds < PollInterval)
            {
                return FileChange.None;
            }
            
            _stopwatch.Restart();
            
            try
            {
                _fileInfo.Refresh();
            }
            catch
            {
                // NoOp
            }

            var exist = _fileInfo.Exists;
            
            switch (_exist)
            {
                case false when exist:
                    _exist = exist;
                    return FileChange.Created;
                case true when !exist:
                    _exist = exist;
                    return FileChange.Deleted;
            }

            var length = _fileInfo.Length;

            if (_lenght == length)
            {
                return FileChange.None;
            }
            
            _lenght = length;
            return FileChange.Changed;
        }
    }
}