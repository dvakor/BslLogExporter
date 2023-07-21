using LogExporter.Core.Brackets.Values;

namespace LogExporter.Core.Metadata.Elements.Abstraction
{
    public interface IMetadataElement
    {
        ObjectType MetadataType { get; }

        bool IsMatch(BracketsStringValue value);
    }
}