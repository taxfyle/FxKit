// ReSharper disable LoopCanBeConvertedToQuery
namespace FxKit.CompilerServices.CodeGenerators;

/// <summary>
///     Enumerable extensions.
/// </summary>
internal static class EnumerableExtensions
{
    /// <summary>
    ///     Like a regular select but filters out nulls.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
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

    /// <summary>
    ///     Like a regular select but filters out nulls and accepts a context parameter.
    ///     Useful to avoid a closure allocation.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="C"></typeparam>
    /// <returns></returns>
    public static IEnumerable<R> SelectNotNull<T, C, R>(
        this IEnumerable<T> source,
        C context,
        Func<C, T, R?> selector)
    {
        foreach (var item in source)
        {
            var mapped = selector(context, item);
            if (mapped is not null)
            {
                yield return mapped;
            }
        }
    }
}
