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
        source.TryGet(out var value)
            ? selector(value)
            : Option<U>.None;

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
        source.TryGet(out var value)
            ? selector(value)
            : Task.FromResult(Option<U>.None);

    /// <summary>
    ///     Returns the source's Option if it contains a value, otherwise <paramref name="other" /> is returned.
    /// </summary>
    /// <remarks>
    ///     <paramref name="other" /> is eagerly evaluated; if the result of a function is being passed,
    ///     use <see cref="OrElse{T}" />.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<T> Or<T>(
        this Option<T> source,
        Option<T> other)
        where T : notnull =>
        source.TryGet(out _) ? source : other;

    /// <summary>
    ///     Returns the source's Option if it contains a value, otherwise calls <paramref name="fallback" />
    ///     and returns the result.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fallback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<T> OrElse<T>(
        this Option<T> source,
        Func<Option<T>> fallback)
        where T : notnull =>
        source.TryGet(out _) ? source : fallback();

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
        source.TryGet(out var srcValue) && bind(srcValue).TryGet(out var bindValue)
            ? Option<UU>.Some(selector(srcValue, bindValue))
            : Option<UU>.None;

    #endregion
}
