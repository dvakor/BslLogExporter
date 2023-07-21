using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;
using LogExporter.Core.Metadata.Elements.Abstraction;

namespace LogExporter.Core.Metadata.Elements
{
    public class AddPortMetadata : AbstractMetadata
    {
        public override ObjectType MetadataType => ObjectType.AddPort;
        
        public int Port { get; }
        
        public AddPortMetadata(BracketsNodeValue nodeValue) : base(nodeValue)
        {
            Port = nodeValue.Value(1).ToInt();
        }
    }
}