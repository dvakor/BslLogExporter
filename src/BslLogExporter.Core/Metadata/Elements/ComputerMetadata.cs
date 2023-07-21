using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class ComputerMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.Computer;
        
        public string Name { get; }
        
        public ComputerMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Name = nodeValue.Value(1).Value;
        }
    }
}