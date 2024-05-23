using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

// ReSharper disable InconsistentNaming

namespace FxKit;

public static partial class Option
{
    /// <summary>
    ///     Matches on the value held within the <see cref="Option{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="Some"></param>
    /// <param name="None"></param>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static R Match<T, R>(
        this Option<T> source,
        Func<T, R> Some,
        Func<R> None)
        where T : notnull
        where R : notnull => source.Match(Some, None);

    /// <summary>
    ///     Unwraps the option, returning the <typeparamref name="T" />> held within if in a Some state.
    ///     If not, returns null.
    /// </summary>
    /// <remarks>
    ///     No transformers exist for this method since rarely would a nullable type be allowed inside
    ///     another Monad.
    /// </remarks>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToNullableValue<T>(this Option<T> source)
        where T : struct =>
        source.TryGet(out var result) ? result : null;

    /// <summary>
    ///     Unwraps the option, returning the <typeparamref name="T" />> held within if in a Some state.
    ///     If not, returns null.
    /// </summary>
    /// <remarks>
    ///     No transformers exist for this method since rarely would a nullable type be allowed inside
    ///     another Monad.
    /// </remarks>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToNullable<T>(this Option<T> source)
        where T : class =>
        source.TryGet(out var result) ? result : null;

    /// <summary>
    ///     Like <see cref="ToNullable{T}" /> but for tasks.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> ToNullableAsync<T>(this Task<Option<T>> source)
        where T : class =>
        (await source).TryGet(out var result) ? result : null;

    /// <summary>
    ///     Like <see cref="ToNullable{T}" /> but for tasks.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T?> ToNullableAsync<T>(this ValueTask<Option<T>> source)
        where T : class =>
        (await source).TryGet(out var result) ? result : null;

    /// <summary>
    ///     Unwraps the option, returning the <typeparamref name="T" />> held within if in a Some state.
    ///     If not, throws an <see cref="InvalidOperationException" />. Only use when you are certain that
    ///     the option is in a Some state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="exceptionMessage">If specified, will use as the exception message.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static T Unwrap<T>(
        this Option<T> source,
        string? exceptionMessage = null)
        where T : notnull =>
        source.Match(
            ok => ok,
            () =>
                throw new InvalidOperationException(
                    exceptionMessage ?? "The option did not contain a value."));

    /// <summary>
    ///     Unwraps the option, returning the <typeparamref name="T" />> held within if in a Some state.
    ///     If not, returns the <paramref name="fallback" />
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static T UnwrapOr<T>(
        this Option<T> source,
        T fallback)
        where T : notnull => source.Match(ok => ok, () => fallback);

    /// <summary>
    ///     Unwraps the option, returning the <typeparamref name="T" />> held within if in a Some state.
    ///     If not, returns the value produced by calling <paramref name="fallback" />
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fallback">The fallback selector.</param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static T UnwrapOrElse<T>(
        this Option<T> source,
        Func<T> fallback)
        where T : notnull => source.Match(ok => ok, fallback);

    /// <summary>
    ///     Returns a list of the <c>Some</c> values.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IReadOnlyList<T> Somes<T>(
        this IEnumerable<Option<T>> source)
        where T : notnull
    {
        var result = source.TryGetNonEnumeratedCount(out var count) ? new List<T>(count) : new List<T>();
        foreach (var option in source)
        {
            if (option.TryGet(out var some))
            {
                result.Add(some);
            }
        }

        return result;
    }
}
