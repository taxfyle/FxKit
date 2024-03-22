namespace FxKit.CompilerServices.CodeGenerators;

public static class EnumerableExtensions
{
    public static IEnumerable<R> SelectNotNull<T, R>(
        this IEnumerable<T> source,
        Func<T, R?> selector)
    {
        foreach (var item in source)
        {
            var mapped = selector(item);
            if (mapped is not null)
            {
                yield return mapped;
            }
        }
    }
}
