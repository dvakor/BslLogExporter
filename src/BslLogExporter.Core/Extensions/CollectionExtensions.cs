namespace LogExporter.Core.Extensions;

public static class CollectionExtensions
{
    public static int IndexOf<T>(this T[] array, T item)
    {
        return Array.IndexOf(array, item);
    }

    public static int LastIndex<T>(this T[] array)
    {
        return array.Length - 1;
    }
}