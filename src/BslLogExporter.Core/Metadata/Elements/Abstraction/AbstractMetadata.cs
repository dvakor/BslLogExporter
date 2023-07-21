using LogExporter.Core.Brackets.Values;
using LogExporter.Core.Extensions;

namespace LogExporter.Core.Metadata.Elements.Abstraction
{
    public abstract class AbstractMetadata : IMetadataElement
    {
        public abstract ObjectType MetadataType { get; }
        
        public string InnerId { get; }
        
        protected AbstractMetadata(BracketsNodeValue nodeValue)
        {
            InnerId = nodeValue.Last().As<BracketsStringValue>().Value;
        }
        
        public virtual bool IsMatch(BracketsStringValue value)
        {
            return InnerId.Equals(value.Value);
        }
    }
}