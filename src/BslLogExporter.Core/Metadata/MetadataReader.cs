using LogExporter.Core.Brackets;
using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata
{
    public sealed class MetadataReader : BracketsFileReader
    {
        private readonly List<IMetadataElement> _loadedMetadata = new();
        private readonly SemaphoreSlim _semaphore = new(1);

        private readonly Dictionary<Type, Dictionary<BracketsStringValue, IMetadataElement?>> _searchCache = new();

        public IReadOnlyCollection<IMetadataElement> LoadedMetadata => _loadedMetadata;
        
        public MetadataReader(string filePath) : base(filePath)
        {
        }
        
        public T? FindMetadata<T>(BracketsStringValue value, CancellationToken token) where T : IMetadataElement
        {
            var mdType = typeof(T);

            if (_searchCache.TryGetValue(mdType, out var dict) &&
                dict.TryGetValue(value, out var res))
            {
                return (T?)res;
            }
            
            var md = _loadedMetadata.OfType<T>().FirstOrDefault(x => x.IsMatch(value));

            if (md == null)
            {
                ReadMetadata(token);    
            }
            
            md = _loadedMetadata.OfType<T>().FirstOrDefault(x => x.IsMatch(value));

            if (!_searchCache.ContainsKey(mdType))
            {
                _searchCache.Add(mdType, new Dictionary<BracketsStringValue, IMetadataElement?>());
            }

            _searchCache[mdType].Add(value, md);

            return md;
        }

        private void ReadMetadata(CancellationToken token)
        {
            _semaphore.Wait(token);
            
            try
            {
                var currentNode = ReadNext(token);
            
                while (currentNode != null)
                {
                    _loadedMetadata.Add(CreateElement(currentNode));
                    currentNode = ReadNext(token);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static IMetadataElement CreateElement(BracketsNodeValue node)
        {
            var typeId = node.Value(0).ToInt();

            var elementType = Enum.IsDefined(typeof(ObjectType), typeId) ? (ObjectType)typeId : ObjectType.None;

            return elementType switch
            {
                ObjectType.Application => new ApplicationMetadata(node),
                ObjectType.Computer => new ComputerMetadata(node),
                ObjectType.Event => new EventMetadata(node),
                ObjectType.Object => new ObjectMetadata(node),
                ObjectType.Server => new ServerMetadata(node),
                ObjectType.User => new UserMetadata(node),
                ObjectType.AddPort => new AddPortMetadata(node),
                ObjectType.MainPort => new MainPortMetadata(node),
                _ => new UnknownMetadata(node)
            };
        }
    }
}