using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class UnknownMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.None;
        
        public BracketsNodeValue Node { get; }
        
        public UnknownMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Node = nodeValue;
        }
        
        public override bool IsMatch(BracketsStringValue value)
        {
            return false;
        }
    }
}