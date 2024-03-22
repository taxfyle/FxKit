using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit.Extensions;

/// <summary>
///     Extensions for <see cref="IEnumerable{T}" />.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     If the <paramref name="source" /> has at least one item, returns it as <c>Some</c>.
    ///     Otherwise, returns <see cref="Option{T}.None" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Option<T> FirstOrNone<T>(
        this IEnumerable<T> source)
        where T : notnull
    {
        foreach (var element in source)
        {
            return element;
        }

        return None;
    }

    /// <summary>
    ///     If the <paramref name="source" /> has at least one item that matches
    ///     <paramref name="predicate" />, returns it as <c>Some</c>.
    ///     Otherwise, returns <see cref="Option{T}.None" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Option<T> FirstOrNone<T>(
        this IEnumerable<T> source,
        Func<T, bool> predicate)
        where T : notnull
    {
        foreach (var element in source)
        {
            if (predicate(element))
            {
                return element;
            }
        }

        return None;
    }

    /// <summary>
    ///     NRT-safe way to filter out non-nulls.
    /// </summary>
    /// <param name="source">The source enumerable to filter out.</param>
    /// <typeparam name="T">Enumerable item type.</typeparam>
    /// <returns>An enumerable with nulls filtered out, including the resulting type.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ^ Can't because we lose the non-null narrowing
        foreach (var item in source)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    ///     Maps an <see cref="IEnumerable{T}" /> to a <see cref="IReadOnlyList{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
    {
        IReadOnlyList<T> result = source.ToList();
        return result;
    }

    /// <summary>
    ///     Maps an <see cref="IEnumerable{T}" /> to a <see cref="IReadOnlySet{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlySet<T> ToReadOnlySet<T>(this IEnumerable<T> source)
    {
        IReadOnlySet<T> result = source.ToHashSet();
        return result;
    }

    /// <summary>
    ///     Functor `Map` method - same as Select.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static IEnumerable<U> Map<T, U>(this IEnumerable<T> source, Func<T, U> selector) =>
        source.Select(selector);
}
