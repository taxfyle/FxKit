using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class TaskExtensions
{
    /// <summary>
    ///     Maps a task's result to another task of a different value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<R> FlatMap<T, R>(this Task<T> source, Func<T, Task<R>> selector) =>
        await selector(await source);

    /// <summary>
    ///     Maps a task's result to another task of a different value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<R> FlatMap<T, R>(
        this ValueTask<T> source,
        Func<T, Task<R>> selector) =>
        await selector(await source);
}
