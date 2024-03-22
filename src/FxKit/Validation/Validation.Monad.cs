using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Validation
{
    /// <summary>
    ///     Maps the Valid value to a <see cref="Validation{T,E}" /> that in unwrapped.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TNewValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<TNewValid, TInvalid> FlatMap<TValid, TNewValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, Validation<TNewValid, TInvalid>> selector)
        where TValid : notnull
        where TNewValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: selector,
            Invalid: Validation<TNewValid, TInvalid>.Invalid);

    /// <summary>
    ///     Asynchronously maps the source's Some value to another Option that is unwrapped.
    /// </summary>
    /// <remarks>
    ///     This method is named `FlatMapAsync` since it's a friendlier name, but it's nothing but a
    ///     `TraverseFlatMap` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TNewValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Validation<TNewValid, TInvalid>> FlatMapAsync<TValid, TNewValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, Task<Validation<TNewValid, TInvalid>>> selector)
        where TValid : notnull
        where TNewValid : notnull
        where TInvalid : notnull =>
        source.Match(
            Valid: selector,
            Invalid: errs => Validation<TNewValid, TInvalid>.Invalid(errs).ToTask());

    #region LINQ

    /// <inheritdoc cref="FlatMap{T,E,U}" />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<TNewValid, TInvalid> SelectMany<TValid, TInvalid, TNewValid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, Validation<TNewValid, TInvalid>> selector)
        where TInvalid : notnull
        where TNewValid : notnull
        where TValid : notnull =>
        FlatMap(source, selector);

    /// <summary>
    ///     Allows using LINQ.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="bind"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <typeparam name="TBind"></typeparam>
    /// <typeparam name="TNewValid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<TNewValid, TInvalid> SelectMany<TValid, TInvalid, TBind, TNewValid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, Validation<TBind, TInvalid>> bind,
        Func<TValid, TBind, TNewValid> selector)
        where TInvalid : notnull
        where TBind : notnull
        where TValid : notnull
        where TNewValid : notnull =>
        source.Match(
            b => bind(b).Select(r => selector(b, r)),
            Validation<TNewValid, TInvalid>.Invalid);

    #endregion
}
