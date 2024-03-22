using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Option
{
    /// <summary>
    ///     Maps the Some value using the given <paramref name="selector" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<U> Map<T, U>(
        this Option<T> source,
        Func<T, U> selector)
        where T : notnull
        where U : notnull =>
        source.Match(
            Some: v => Some(selector(v)),
            None: () => Option<U>.None);

    /// <summary>
    ///     Asynchronously maps the Some value using the given <paramref name="selector" />.
    /// </summary>
    /// <remarks>
    ///     This method is named `MapAsync` since it's a friendlier name, but it's nothing but a
    ///     `Traverse` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Option<U>> MapAsync<T, U>(
        this Option<T> source,
        Func<T, Task<U>> selector)
        where T : notnull
        where U : notnull =>
        source.Match(
            Some: async v => Some(await selector(v)),
            None: () => Option<U>.None.ToTask());

    #region LINQ

    /// <inheritdoc cref="Map{T,U}" />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<U> Select<T, U>(
        this Option<T> source,
        Func<T, U> selector)
        where T : notnull
        where U : notnull =>
        Map(source, selector);

    #endregion
}
