using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FxKit.CompilerServices;

namespace FxKit.Extensions;

/// <summary>
///     List extensions.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    ///     Like <c>Enumerable.Select</c> but optimized for lists.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IReadOnlyList<R> Map<T, R>(this IReadOnlyList<T> source, Func<T, R> selector)
    {
        var result = new List<R>();

        CollectionsMarshal.SetCount(result, source.Count);
        var span = CollectionsMarshal.AsSpan(result);
        for (var i = 0; i < source.Count; i++)
        {
            span[i] = selector(source[i]);
        }

        return result;
    }

    /// <summary>
    ///     Like <c>Enumerable.FirstOrDefault</c> but optimized for lists.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FirstOrDefault<T>(this IReadOnlyList<T> source) =>
        source.Count == 0 ? default : source[0];

    /// <summary>
    ///     Like <c>WhereNotNull</c> but returning a list.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyList<T> WhereNotNull<T>(this IReadOnlyList<T?> source)
    {
        var result = new List<T>();
        result.AddRange(EnumerableExtensions.WhereNotNull(source));
        return result;
    }

    /// <summary>
    ///     Maps an <see cref="IReadOnlyList{T}" /> to a <see cref="IReadOnlySet{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IReadOnlySet<T> ToReadOnlySet<T>(this IReadOnlyList<T> source)
    {
        IReadOnlySet<T> result = source.ToHashSet();
        return result;
    }

    /// <summary>
    ///     Returns <c>Some</c> with the list if it is not empty; <c>None</c> otherwise.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<IReadOnlyList<T>> ToNonEmptyList<T>(this IReadOnlyList<T> source) =>
        source.Count == 0 ? None : Some(source);
}
