using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class TaskExtensions
{
    /// <summary>
    ///     Maps a task's result to another value and returns a task for it.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<R> Map<T, R>(this Task<T> source, Func<T, R> selector) =>
        selector(await source);

    /// <summary>
    ///     Maps a task's result to another value and returns a task for it.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<R> Map<T, R>(this ValueTask<T> source, Func<T, R> selector) =>
        selector(await source);

    /// <summary>
    ///     Maps a result-less task's completion to a value and returns a task for it.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<R> Map<R>(this Task source, Func<R> selector)
    {
        await source;
        return selector();
    }

    /// <summary>
    ///     Maps a result-less task's completion to a value and returns a task for it.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<R> Map<R>(this ValueTask source, Func<R> selector)
    {
        await source;
        return selector();
    }
}
