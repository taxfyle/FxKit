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
        source.TryGet(out var ok, out var err)
            ? selector(ok)
            : Result<TNewOk, TErr>.Err(err);

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
        where TNewOk : notnull =>
        source.TryGet(out var ok, out var err)
            ? selector(ok)
            : Task.FromResult(Result<TNewOk, TErr>.Err(err));

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
        source.TryGet(out var ok, out var err)
            ? Result<TOk, TNewErr>.Ok(ok)
            : selector(err);

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
        where TNewErr : notnull =>
        source.TryGet(out var ok, out var err)
            ? Task.FromResult(Result<TOk, TNewErr>.Ok(ok))
            : selector(err);

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
        source.TryGet(out var srcOk, out var srcErr)
            ? bind(srcOk).TryGet(out var bOk, out var bErr)
                ? Result<TNewOk, TErr>.Ok(selector(srcOk, bOk))
                : Result<TNewOk, TErr>.Err(bErr)
            : Result<TNewOk, TErr>.Err(srcErr);

    #endregion

    #region Ensure

    /// <summary>
    /// Returns a new failure result if the predicate is false. Otherwise, returns the original result.
    /// </summary>
    /// <typeparam name="TOk">The type of the value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error in the result.</typeparam>
    /// <param name="source">The input <see cref="Result{TOk,TErr}"/>.</param>
    /// <param name="predicate">The predicate to be evaluated on the value.</param>
    /// <param name="error">The default error value to return if the check fails.</param>
    /// <returns>A <see cref="Result{TOk,TErr}"/> containing either the original value or the specified error.</returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TOk, TErr> Ensure<TOk, TErr>(
        this Result<TOk, TErr> source,
        Func<TOk, bool> predicate,
        TErr error)
        where TOk : notnull
        where TErr : notnull =>
        source.TryGet(out var value, out _)
            ? predicate(value) ? source : Err<TOk, TErr>(error)
            : source;

    /// <summary>
    /// Returns a new failure result if the predicate is false. Otherwise, returns the original result.
    /// </summary>
    /// <typeparam name="TOk">The type of the value in the result.</typeparam>
    /// <typeparam name="TErr">The type of the error in the result.</typeparam>
    /// <param name="source">The input <see cref="Result{TOk,TErr}"/>.</param>
    /// <param name="predicate">The predicate to be evaluated on the value.</param>
    /// <param name="error">A function that provides the default error value to return if the check fails.</param>
    /// <returns>A <see cref="Result{TOk,TErr}"/> containing either the original value or the specified error.</returns>
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TOk, TErr> Ensure<TOk, TErr>(
        this Result<TOk, TErr> source,
        Func<TOk, bool> predicate,
        Func<TErr> error)
        where TOk : notnull
        where TErr : notnull =>
        source.TryGet(out var value, out _)
            ? predicate(value) ? source : Err<TOk, TErr>(error())
            : source;

    #endregion
}
