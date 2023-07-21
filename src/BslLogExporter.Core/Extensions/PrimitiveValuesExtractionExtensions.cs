using System.Globalization;
using LogExporter.Core.Brackets.Values;

namespace LogExporter.Core.Extensions
{
    public static class PrimitiveValuesExtractionExtensions
    {
        private const string DtFormat = "yyyyMMddHHmmss";
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static long ToLong(this BracketsStringValue value)
        {
            return long.Parse(value.Value);
        }
        
        public static int ToInt(this BracketsStringValue value)
        {
            return int.Parse(value.Value);
        }
        
        public static DateTime ToDateTime(this BracketsStringValue value)
        {
            if (DateTime.TryParseExact(value.Value, DtFormat, Culture, default, out var result))
            {
                return result;
            }

            var number = value.ToBase16Number();

            if (number == 0)
            {
                return new DateTime();
            }
            
            var seconds = number / 10000;

            var timeSpan = TimeSpan.FromSeconds(seconds);

            return new DateTime().Add(timeSpan);
        }
        
        public static long ToBase16Number(this BracketsStringValue value)
        {
            return Convert.ToInt64(value.Value, 16);
        }
    }
}