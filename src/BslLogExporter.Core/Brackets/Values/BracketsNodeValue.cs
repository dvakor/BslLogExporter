using System.Collections;

namespace LogExporter.Core.Brackets.Values
{
    public record BracketsNodeValue : IBracketsValue, IReadOnlyCollection<IBracketsValue>
    {
        private readonly List<IBracketsValue> _elements = new();

        public BracketsValueType ValueType => BracketsValueType.Node;
        
        public long StartPosition { get; init; }
        
        public long EndPosition { get; internal set; }
        
        public int Count => _elements.Count;

        public IBracketsValue this[int index] => _elements[index];

        public void AddStringValue(string str)
        {
            _elements.Add(new BracketsStringValue(str));
        }
        
        public void AddNodeValue(BracketsNodeValue value)
        {
            _elements.Add(value);
        }
        
        public override string ToString()
        {
            return $"{{{string.Join(",", _elements)}}}";
        }

        public IEnumerator<IBracketsValue> GetEnumerator() => _elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();
    }
}