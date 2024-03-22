using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class TaskExtensions
{
    /// <summary>
    ///     Calls the <see cref="callback" /> with the result of the task but returns the original task
    ///     value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> Do<T>(this Task<T> source, Action<T> callback)
    {
        var result = await source;
        callback(result);
        return result;
    }

    /// <summary>
    ///     Calls the <see cref="callback" /> with the result of the task but returns the original task
    ///     value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> Do<T>(this ValueTask<T> source, Action<T> callback)
    {
        var result = await source;
        callback(result);
        return result;
    }

    /// <summary>
    ///     Calls the <see cref="callback" /> when the task completes.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task Do(this Task source, Action callback)
    {
        await source;
        callback();
    }

    /// <summary>
    ///     Calls the <see cref="callback" /> when the task completes.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask Do(this ValueTask source, Action callback)
    {
        await source;
        callback();
    }

    /// <summary>
    ///     Calls the <see cref="callback" /> with the result of the task but returns the original task
    ///     value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> DoAsync<T>(this Task<T> source, Func<T, Task> callback)
    {
        var result = await source;
        await callback(result);
        return result;
    }

    /// <summary>
    ///     Calls the <see cref="callback" /> with the result of the task but returns the original task
    ///     value.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> DoAsync<T>(this ValueTask<T> source, Func<T, Task> callback)
    {
        var result = await source;
        await callback(result);
        return result;
    }
}
