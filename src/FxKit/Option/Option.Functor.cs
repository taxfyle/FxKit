using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Option
{
    /// <summary>
    ///     Maps the Some value using the given <paramref name="selector" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<U> Map<T, U>(
        this Option<T> source,
        Func<T, U> selector)
        where T : notnull
        where U : notnull =>
        source.TryGet(out var value)
            ? Option<U>.Some(selector(value))
            : Option<U>.None;

    /// <summary>
    ///     Asynchronously maps the Some value using the given <paramref name="selector" />.
    /// </summary>
    /// <remarks>
    ///     This method is named `MapAsync` since it's a friendlier name, but it's nothing but a
    ///     `Traverse` operation to a `Task`.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Task<Option<U>> MapAsync<T, U>(
        this Option<T> source,
        Func<T, Task<U>> selector)
        where T : notnull
        where U : notnull
    {
        return source.TryGet(out var value)
            ? HandleSome(value, selector)
            : Task.FromResult(Option<U>.None);

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static async Task<Option<U>> HandleSome(T value, Func<T, Task<U>> selector) =>
            Option<U>.Some(await selector(value).ConfigureAwait(false));
    }

    #region LINQ

    /// <inheritdoc cref="Map{T,U}" />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<U> Select<T, U>(
        this Option<T> source,
        Func<T, U> selector)
        where T : notnull
        where U : notnull =>
        Map(source, selector);

    #endregion
}
