﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Result
{
    /// <summary>
    ///     Maps the source's Ok value to another one in case the result is in an Ok state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewOk"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, TNewOk> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewOk : notnull =>
        source.Match(ok => Result<TNewOk, TErr>.Ok(selector(ok)), Result<TNewOk, TErr>.Err);

    /// <summary>
    ///     Asynchronously maps the source's Ok value to another one in case the result is in an Ok state.
    /// </summary>
    /// <remarks>
    ///     This method is named `MapAsync` since it's a friendlier name, but it's nothing but a
    ///     `Traverse` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewOk"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Result<TNewOk, TErr>> MapAsync<TOk, TErr, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, Task<TNewOk>> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewOk : notnull =>
        source.Match<Task<Result<TNewOk, TErr>>>(
            async ok => Result<TNewOk, TErr>.Ok(await selector(ok)),
            e => Result<TNewOk, TErr>.Err(e).ToTask());

    /// <summary>
    ///     Maps the source's Err value to another one in case the result is in an Err state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerNonUserCode]
    [GenerateTransformer]
    public static Result<TOk, TNewErr> MapErr<TOk, TErr, TNewErr>(
        this Result<TOk, TErr> source,
        Func<TErr, TNewErr> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewErr : notnull
    {
        return source.Match(
            Result<TOk, TNewErr>.Ok,
            OnErr);

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Result<TOk, TNewErr> OnErr(TErr e) => Result<TOk, TNewErr>.Err(selector(e));
    }

    /// <summary>
    ///     Asynchronously maps the source's Err value to another one in case the result is in an Err state.
    /// </summary>
    /// <remarks>
    ///     This method is named `MapErrAsync` since it's a friendlier name, but it's nothing but a
    ///     `TraverseErr` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <typeparam name="TNewErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Result<TOk, TNewErr>> MapErrAsync<TOk, TErr, TNewErr>(
        this Result<TOk, TErr> source,
        Func<TErr, Task<TNewErr>> selector)
        where TOk : notnull
        where TErr : notnull
        where TNewErr : notnull =>
        source.Match<Task<Result<TOk, TNewErr>>>(
            ok => Result<TOk, TNewErr>.Ok(ok).ToTask(),
            async e => Result<TOk, TNewErr>.Err(await selector(e)));

    /// <inheritdoc cref="Map{T,E,U}" />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TNewOk, TErr> Select<TOk, TErr, TNewOk>(
        this Result<TOk, TErr> source,
        Func<TOk, TNewOk> selector)
        where TNewOk : notnull where TErr : notnull where TOk : notnull =>
        Map(source, selector);
}
