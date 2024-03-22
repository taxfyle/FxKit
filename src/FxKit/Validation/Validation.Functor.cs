using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Validation
{
    /// <summary>
    ///     Maps the Valid value using the given <see cref="selector" />.
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
    public static Validation<TNewValid, TInvalid> Map<TValid, TInvalid, TNewValid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, TNewValid> selector)
        where TValid : notnull
        where TNewValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: v => Validation<TNewValid, TInvalid>.Valid(selector(v)),
            Invalid: errs => Validation<TNewValid, TInvalid>.Invalid(errs));

    /// <summary>
    ///     Asynchronously maps the Valid value using the given <paramref name="selector" />.
    /// </summary>
    /// <remarks>
    ///     This method is named `MapAsync` since it's a friendlier name, but it's nothing but a
    ///     `Traverse` operation to a `Task`.
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
    public static Task<Validation<TNewValid, TInvalid>> MapAsync<TValid, TNewValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, Task<TNewValid>> selector)
        where TValid : notnull
        where TNewValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: async v => Validation<TNewValid, TInvalid>.Valid(await selector(v)),
            Invalid: errs => Validation<TNewValid, TInvalid>.Invalid(errs).ToTask());

    /// <summary>
    ///     Maps the source's Err value to another one in case the Validation is in an Invalid state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <typeparam name="TNewInvalid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<TValid, TNewInvalid> MapInvalid<TValid, TInvalid, TNewInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<TInvalid, TNewInvalid> selector)
        where TValid : notnull
        where TInvalid : notnull
        where TNewInvalid : notnull
    {
        return source.Match(
            Validation<TValid, TNewInvalid>.Valid,
            OnErr);

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Validation<TValid, TNewInvalid> OnErr(IEnumerable<TInvalid> e) =>
            Validation<TValid, TNewInvalid>.Invalid(
                e.Aggregate(
                    Enumerable.Empty<TNewInvalid>(),
                    (errors, err) => errors.Append(selector(err))));
    }

    /// <summary>
    ///     Maps the source's Err value to another one in case the Validation is in an Invalid state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <typeparam name="TNewInvalid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Validation<TValid, TNewInvalid>> MapErrAsync<TValid, TInvalid, TNewInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<IEnumerable<TInvalid>, Task<IEnumerable<TNewInvalid>>> selector)
        where TValid : notnull
        where TInvalid : notnull
        where TNewInvalid : notnull
    {
        return source.Match(
            t => Validation<TValid, TNewInvalid>.Valid(t).ToTask(),
            OnErr);

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task<Validation<TValid, TNewInvalid>> OnErr(IEnumerable<TInvalid> e) =>
            Validation<TValid, TNewInvalid>.Invalid(await selector(e));
    }

    // These mapping methods are only available on `Validation.Valid<T>` and are here
    // for type inference purposes with the Valid-side `Apply` methods.

    #region Valid-Side Map

    /// <summary>
    ///     Maps the Valid value using the given <see cref="selector" />.
    /// </summary>
    /// <remarks>
    ///     Should not be called.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<U> Map<T, U>(
        this Valid<T> source,
        Func<T, U> selector)
        where T : notnull
        where U : notnull
        => new(selector(source.Value));

    #endregion

    #region LINQ

    /// <inheritdoc cref="Map{T,E,U}" />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<TNewValid, TInvalid> Select<TValid, TInvalid, TNewValid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, TNewValid> selector)
        where TNewValid : notnull where TInvalid : notnull where TValid : notnull =>
        Map(source, selector);

    #endregion
}
