namespace LogExporter.Core.Brackets.Values
{
    public record BracketsStringValue(string Value) : IBracketsValue
    {
        public BracketsValueType ValueType => BracketsValueType.String;

        public override string ToString() => Value;
    }
}