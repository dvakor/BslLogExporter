using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class ObjectMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.Object;

        public string Id { get; }
        
        public string Name { get; }
        
        public ObjectMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Id = nodeValue.Value(1).Value;
            Name = nodeValue.Value(2).Value;
        }
    }
}