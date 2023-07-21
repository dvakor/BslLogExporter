using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class MainPortMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.MainPort;
        
        public int Port { get; }
        
        public MainPortMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Port = nodeValue.Value(1).ToInt();
        }
    }
}