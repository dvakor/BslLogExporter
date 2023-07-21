using LogExporter.Core.Brackets.Values;

namespace LogExporter.Core.Extensions
{
    public static class ContextualValuesExtractionExtensions
    {
        public static string ToTransactionStatus(this BracketsStringValue value)
        {
            return value.Value switch
            {
                "U" => "Зафиксирована",
                "C" => "Отменена",
                "R" => "Не завершена",
                "N" => "Нет транзакции",
                _ => "Неизвестно"
            };
        }

        public static string ToSeverity(this BracketsStringValue value)
        {
            return value.Value switch
            {
                "I" => "Информация",
                "E" => "Ошибка",
                "W" => "Предупреждение",
                "N" => "Примечание",
                _ => "Неизвестно"
            };
        }

        public static string ToAdditionalData(this BracketsNodeValue value)
        {
            var type = value.Value(0).Value;

            return type switch
            {
                "R" or "S" => value.Value(1).Value,
                "B" => value.Value(1).Value.Equals("1").ToString(),
                "C" => GetComplexAdditionalData(value.Node(1)),
                _ => string.Empty
            };
        }

        private static string GetComplexAdditionalData(BracketsNodeValue node)
        {
            throw new NotImplementedException();
        }
    }
}