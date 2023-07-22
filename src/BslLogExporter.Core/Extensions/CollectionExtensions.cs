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

    public static bool DiffersFrom<T>(this ICollection<T> left, ICollection<T> right) where T : IEquatable<T>
    {
        return left.Except(right).Any() && right.Except(left).Any();
    }
}