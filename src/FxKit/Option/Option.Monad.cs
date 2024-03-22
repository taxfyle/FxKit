using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Option
{
    /// <summary>
    ///     Maps the source's Some value to another Option that is unwrapped.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<U> FlatMap<T, U>(
        this Option<T> source,
        Func<T, Option<U>> selector)
        where T : notnull
        where U : notnull =>
        source.Match(
            Some: v => selector(v),
            None: () => Option<U>.None);

    /// <summary>
    ///     Asynchronously maps the source's Some value to another Option that is unwrapped.
    /// </summary>
    /// <remarks>
    ///     This method is named `FlatMapAsync` since it's a friendlier name, but it's nothing but a
    ///     `TraverseFlatMap` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Option<U>> FlatMapAsync<T, U>(
        this Option<T> source,
        Func<T, Task<Option<U>>> selector)
        where T : notnull
        where U : notnull =>
        source.Match(
            Some: selector,
            None: () => Option<U>.None.ToTask());

    #region LINQ

    /// <inheritdoc cref="FlatMap{T,U}" />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<U> SelectMany<T, U>(
        this Option<T> source,
        Func<T, Option<U>> selector)
        where T : notnull
        where U : notnull =>
        FlatMap(source, selector);

    /// <summary>
    ///     Allows using LINQ.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="bind"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="UU"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<UU> SelectMany<T, U, UU>(
        this Option<T> source,
        Func<T, Option<U>> bind,
        Func<T, U, UU> selector)
        where T : notnull
        where U : notnull
        where UU : notnull =>
        source.Match(
            Some: b => bind(b).Select(v => selector(b, v)),
            None: () => Option<UU>.None);

    #endregion
}
