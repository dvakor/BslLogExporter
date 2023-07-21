using LogExporter.Core.Brackets.Values;

namespace LogExporter.Core.Extensions
{
    public static class ItemSelectionExtensions
    {
        public static BracketsStringValue Value(this BracketsNodeValue value, int index)
        {
            try
            {
                var selectedValue = value.Item(index);
                selectedValue.EnsureIsType(BracketsValueType.String);
                return selectedValue.As<BracketsStringValue>();
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка получения значения по индексу {index}", e);
            }
        }
        
        public static BracketsNodeValue Node(this BracketsNodeValue value, int index)
        {
            try
            {
                var selectedValue = value.Item(index);
                selectedValue.EnsureIsType(BracketsValueType.Node);
                return selectedValue.As<BracketsNodeValue>();
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка получения ноды по индексу {index}", e);
            }
        }
        
        public static T As<T>(this IBracketsValue value)
        {
            if (value is T castedValue)
            {
                return castedValue;
            }

            throw new InvalidCastException();
        }

        private static IBracketsValue Item(this BracketsNodeValue value, int index)
        {
            value.EnsureIsType(BracketsValueType.Node);
            return value[index];
        }

        private static void EnsureIsType(this IBracketsValue value, BracketsValueType type)
        {
            if (value.ValueType != type)
            {
                throw new InvalidOperationException($"Ожидался тип {type}, получен {value.ValueType}");
            }
        }
    }
}