using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Result
{
    #region FlatMap

    /// <summary>
    ///     Maps the source's Ok value to another Result that is unwrapped.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewOk"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TNewOk, TErr> FlatMap<TOk, TErr, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, Result<TNewOk, TErr>> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewOk : notnull =>
        source.Match(
            selector,
            Result<TNewOk, TErr>.Err);

    /// <summary>
    ///     Asynchronously maps the source's Ok value to another Result that is unwrapped.
    /// </summary>
    /// <remarks>
    ///     This method is named `FlatMapAsync` since it's a friendlier name, but it's nothing but a
    ///     `TraverseFlatMap` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewOk"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Result<TNewOk, TErr>> FlatMapAsync<TOk, TErr, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, Task<Result<TNewOk, TErr>>> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewOk : notnull
    {
        return source.Match(
            selector,
            OnErr);

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task<Result<TNewOk, TErr>> OnErr(TErr arg) => Result<TNewOk, TErr>.Err(arg).ToTask();
    }

    #endregion

    #region FlatMapErr

    /// <summary>
    ///     Maps the source's Err value to another Result that is unwrapped.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TOk, TNewErr> FlatMapErr<TOk, TErr, TNewErr>(
        this Result<TOk, TErr> source,
        Func<TErr, Result<TOk, TNewErr>> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewErr : notnull =>
        source.Match(
            Result<TOk, TNewErr>.Ok,
            selector);

    /// <summary>
    ///     Asynchronously maps the source's Err value to another Result that is unwrapped.
    /// </summary>
    /// <remarks>
    ///     This method is named `FlatMapErrAsync` since it's a friendlier name, but it's nothing but a
    ///     `TraverseFlatMapErr` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Result<TOk, TNewErr>> FlatMapErrAsync<TOk, TErr, TNewErr>(
        this Result<TOk, TErr> source,
        Func<TErr, Task<Result<TOk, TNewErr>>> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewErr : notnull
    {
        return source.Match(
            OnOk,
            selector);

        [DebuggerHidden]
        [StackTraceHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task<Result<TOk, TNewErr>> OnOk(TOk arg) => Result<TOk, TNewErr>.Ok(arg).ToTask();
    }

    #endregion

    #region LINQ

    /// <inheritdoc cref="FlatMap{T,E,U}" />
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TNewOk, TErr> SelectMany<TOk, TErr, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, Result<TNewOk, TErr>> selector)
        where TErr : notnull
        where TNewOk : notnull
        where TOk : notnull =>
        FlatMap(source, selector);

    /// <summary>
    ///     Allows using LINQ.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="bind"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TBind"></typeparam>
    /// <typeparam name="TNewOk"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TNewOk, TErr> SelectMany<TOk, TErr, TBind, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, Result<TBind, TErr>> bind,
        Func<TOk, TBind, TNewOk> selector)
        where TErr : notnull
        where TBind : notnull
        where TOk : notnull
        where TNewOk : notnull =>
        source.Match(
            b => bind(b).Select(r => selector(b, r)),
            Result<TNewOk, TErr>.Err);

    #endregion
}
