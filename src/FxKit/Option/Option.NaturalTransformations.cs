using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

// ReSharper disable RedundantNameQualifier

namespace FxKit;

public static partial class Option
{
    /// <summary>
    /// Returns a result containing the value from an <see cref="Option{T}"/> if it exists.
    /// Otherwise, returns a result with the specified error value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <typeparam name="R">The type of the error in the result.</typeparam>
    /// <param name="source">The input <see cref="Option{T}"/>.</param>
    /// <param name="selector">A function that provides the error value to return if the option is empty.</param>
    /// <returns>A <see cref="Result{TOk,TErr}"/> containing either the value or the specified error.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<T, R> OkOrElse<T, R>(
        this Option<T> source,
        Func<R> selector)
        where T : notnull
        where R : notnull =>
        source.TryGet(out var value)
            ? Result<T, R>.Ok(value)
            : Result<T, R>.Err(selector());

    /// <summary>
    /// Returns a result containing the value from an <see cref="Option{T}"/> if it exists.
    /// Otherwise, returns a result with the specified error value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <typeparam name="R">The type of the error in the result.</typeparam>
    /// <param name="source">The input <see cref="Option{T}"/>.</param>
    /// <param name="invalidValue">The error value to return if the option is empty.</param>
    /// <returns>A <see cref="Result{TOk,TErr}"/> containing either the value or the specified error.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<T, R> OkOr<T, R>(
        this Option<T> source,
        R invalidValue)
        where T : notnull
        where R : notnull =>
        source.TryGet(out var value)
            ? Result<T, R>.Ok(value)
            : Result<T, R>.Err(invalidValue);

    /// <summary>
    ///     Maps the <see cref="Option{T}" /> to a <see cref="Validation{T,E}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<T, E> ValidOrElse<T, E>(
        this Option<T> source,
        Func<E> selector)
        where T : notnull
        where E : notnull =>
        source.Match(
            Some: Validation<T, E>.Valid,
            None: () => Validation<T, E>.Invalid(selector()));

    /// <summary>
    ///     Maps the <see cref="Option{T}" /> to a <see cref="Validation{T,E}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="invalidValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<T, E> ValidOr<T, E>(this Option<T> source, E invalidValue)
        where E : notnull where T : notnull => source.ValidOrElse(() => invalidValue);

    /// <summary>
    ///     Converts the <see cref="Option{T}" /> into a <see cref="Result{T,E}" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<T, Option.None> ToResult<T>(this Option<T> source)
        where T : notnull
        => source.OkOrElse(() => new Option.None());
}
