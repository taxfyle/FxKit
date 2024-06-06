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
    /// <param name="Some">A function to invoke if the <see cref="Option{T}"/> has a value.</param>
    /// <param name="None">A function to invoke if the <see cref="Option{T}"/> is empty.</param>
    /// <typeparam name="R">The return type of the match functions.</typeparam>
    /// <typeparam name="T">The type of the value contained in the <see cref="Option{T}"/>.</typeparam>
    /// <returns>The result of invoking the <paramref name="Some"/> or the <paramref name="None"/> function.</returns>
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
    /// <param name="source">The input to unwrap value from.</param>
    /// <param name="exceptionMessage">If specified, will use as the exception message.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The option did not contain a value.</exception>
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
    /// Returns the values from a sequence of <see cref="Option{T}"/> where the option has a value.
    /// </summary>
    /// <param name="source">The input sequence of <see cref="Option{T}"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the values from the options with values.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IEnumerable<T> Somes<T>(
        this IEnumerable<Option<T>> source)
        where T : notnull
    {
        foreach (var item in source)
        {
            if (item.TryGet(out var value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Returns the values from a list of <see cref="Option{T}"/> where the option has a value.
    /// </summary>
    /// <param name="source">The input list of <see cref="Option{T}"/>.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing the values from the options with values.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IReadOnlyList<T> Somes<T>(
        this IReadOnlyList<Option<T>> source)
        where T : notnull
    {
        var result = new List<T>(source.Count);
        foreach (var option in source)
        {
            if (option.TryGet(out var some))
            {
                result.Add(some);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the values from a sequence of <see cref="Option{T}"/> where the option has a value.
    /// Applies a selector function to each value and returns the results.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="U">The type of the result after applying the selector function.</typeparam>
    /// <param name="source">The input sequence of <see cref="Option{T}"/>.</param>
    /// <param name="selector">The selector function to apply to each value.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the results of applying the selector function to the values.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IEnumerable<U> SomesMap<T, U>(
        this IEnumerable<Option<T>> source,
        Func<T, U> selector)
        where T : notnull
    {
        foreach (var item in source)
        {
            if (item.TryGet(out var value))
            {
                yield return selector(value);
            }
        }
    }

    /// <summary>
    /// Returns the values from a list of <see cref="Option{T}"/> where the option has a value.
    /// Applies a selector function to each value and returns the results.
    /// </summary>
    /// <typeparam name="T">The type of the value in the option.</typeparam>
    /// <typeparam name="U">The type of the result after applying the selector function.</typeparam>
    /// <param name="source">The input list of <see cref="Option{T}"/>.</param>
    /// <param name="selector">The selector function to apply to each value.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing the results of applying the selector function to the values.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static IReadOnlyList<U> SomesMap<T, U>(
        this IReadOnlyList<Option<T>> source,
        Func<T, U> selector)
        where T : notnull
    {
        var result = new List<U>(source.Count);
        foreach (var option in source)
        {
            if (option.TryGet(out var some))
            {
                result.Add(selector(some));
            }
        }

        return result;
    }
}
