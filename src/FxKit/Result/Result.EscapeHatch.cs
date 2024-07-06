using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

// ReSharper disable InconsistentNaming

namespace FxKit;

public static partial class Result
{
    /// <summary>
    ///     Unwraps the result and returns either the Ok or Error value if they are the same type.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static T UnwrapEither<T>(
        this Result<T, T> source)
        where T : notnull =>
        source.TryGet(out var ok, out var err) ? ok : err;

    /// <summary>
    ///     Unwraps the result and returns the Ok value, or throws an exception otherwise.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="exceptionMessage"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static TOk Unwrap<TOk, TErr>(
        this Result<TOk, TErr> source,
        string? exceptionMessage = "")
        where TOk : notnull
        where TErr : notnull
        => source.TryGet(out var ok, out var err)
            ? ok
            : throw new InvalidOperationException(
                exceptionMessage ?? err.ToString() ?? "The result was in an Error state.");

    /// <summary>
    ///     Unwraps the result, returning the <typeparamref name="TOk" />> held within if in a Ok state.
    ///     If not, returns the <paramref name="fallback" />
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static TOk UnwrapOr<TOk, TErr>(
        this Result<TOk, TErr> source,
        TOk fallback)
        where TOk : notnull
        where TErr : notnull => source.TryGet(out var ok, out _) ? ok : fallback;

    /// <summary>
    ///     Unwraps the result, returning the <typeparamref name="TOk" />> held within if in a Ok state.
    ///     If not, throws the error returned by <paramref name="mapException" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mapException">The exception mapper.</param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static TOk UnwrapOrThrow<TOk, TErr>(
        this Result<TOk, TErr> source,
        Func<TErr, Exception> mapException)
        where TOk : notnull
        where TErr : notnull
        => source.TryGet(out var ok, out var err) ? ok : throw mapException(err);

    /// <summary>
    ///     Unwraps the result, returning the <typeparamref name="TOk" /> held within if in an Ok state.
    ///     If not, executes the <paramref name="fallbackFunction" /> function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static TOk UnwrapOrElse<TOk, TErr>(
        this Result<TOk, TErr> source,
        Func<TOk> fallbackFunction)
        where TOk : notnull
        where TErr : notnull
        => source.TryGet(out var ok, out _) ? ok : fallbackFunction();

    /// <summary>
    ///     Unwraps the result, expecting it to be in an error state.
    ///     If not, throws an <see cref="InvalidOperationException" />. Only use when you are certain that
    ///     the result is in an Error state.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static TErr UnwrapErr<TOk, TErr>(
        this Result<TOk, TErr> source,
        string? exceptionMessage = null)
        where TOk : notnull
        where TErr : notnull
        => source.TryGet(out var ok, out var err)
            ? throw new InvalidOperationException(
                exceptionMessage ?? ok.ToString() ?? "The result was in an Ok state.")
            : err;

    /// <summary>
    ///     Matches on the result, calling either <paramref name="Ok" /> or <paramref name="Err" />
    ///     depending on the state of the result.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="Ok"></param>
    /// <param name="Err"></param>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static R Match<TOk, TErr, R>(
        this Result<TOk, TErr> source,
        Func<TOk, R> Ok,
        Func<TErr, R> Err)
        where TOk : notnull
        where TErr : notnull
        where R : notnull => source.Match(Ok, Err);

    /// <summary>
    ///     Asynchronously matches on the result, calling either <paramref name="Ok" /> or <paramref name="Err" />
    ///     depending on the state of the result.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="Ok"></param>
    /// <param name="Err"></param>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<R> MatchAsync<TOk, TErr, R>(
        this Result<TOk, TErr> source,
        Func<TOk, Task<R>> Ok,
        Func<TErr, Task<R>> Err)
        where TOk : notnull
        where TErr : notnull
        where R : notnull => source.MatchAsync(Ok, Err);
}
