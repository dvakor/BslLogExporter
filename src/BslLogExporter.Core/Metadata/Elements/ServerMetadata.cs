using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class ServerMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.Server;
        
        public string Name { get; }
        
        public ServerMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Name = nodeValue.Value(1).Value;
        }
    }
}