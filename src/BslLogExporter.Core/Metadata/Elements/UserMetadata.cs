using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class UserMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.User;
        
        public string Id { get; }
        
        public string Name { get; }
        
        public UserMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Id = nodeValue.Value(1).Value;
            Name = nodeValue.Value(2).Value;
        }
    }
}