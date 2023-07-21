using Ardalis.GuardClauses;
using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Watchers;

namespace LogExporter.Core.Brackets
{
    public abstract class BracketsFileReader : IDisposable
    {
        private readonly FileWatcher _watcher;
        private BracketsNodeValue? _lastNode;
        private BracketsParser? _bracketsParser;

        public string FilePath { get; }
        
        public string FileName { get; }

        public long CurrentPosition => _bracketsParser?.CurrentPosition ?? 0;

        public long Length => _bracketsParser?.Length ?? 0;
        
        protected BracketsFileReader(string filePath)
        {
            Guard.Against.NullOrWhiteSpace(filePath);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
            
            _watcher = new FileWatcher(filePath);
            ReCreateBracketParser();
        }

        public void SetPosition(long position)
        {
            ReCreateBracketParser(position);
        }
        
        private void ReCreateBracketParser(long? position = default)
        {
            _bracketsParser?.Dispose();
            _bracketsParser = new BracketsParser(FilePath);
            
            if (position.HasValue)
            {
                _bracketsParser.SetPosition(position.Value);   
            }
        }

        protected BracketsNodeValue? ReadNext(CancellationToken token = default)
        {
            var node = _bracketsParser!.GetNextItem(token);

            if (node != null)
            {
                _lastNode = node;
            }
            else
            {
                var changes = _watcher.DetectChanges();

                if (changes != FileChange.Changed)
                {
                    return node;
                }
                
                ReCreateBracketParser(_lastNode?.EndPosition);
                node = _bracketsParser!.GetNextItem(token);
            }

            return node;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bracketsParser?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}