using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class TaskExtensions
{
    /// <summary>
    ///     Casts the array returned by the task into a read-only list.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IReadOnlyList<T>> AsReadOnlyListAsync<T>(
        this Task<T[]> source) => source.Map(static r => (IReadOnlyList<T>)r);

    /// <summary>
    ///     Casts the array returned by the task into a read-only list.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<IReadOnlyList<T>> AsReadOnlyListAsync<T>(
        this ValueTask<T[]> source) => source.Map(static r => (IReadOnlyList<T>)r);

    /// <summary>
    ///     Casts the array returned by the task into an <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IEnumerable<T>> AsEnumerableAsync<T>(
        this Task<T[]> source) => source.Map(static r => (IEnumerable<T>)r);

    /// <summary>
    ///     Casts the array returned by the task into an <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<IEnumerable<T>> AsEnumerableAsync<T>(
        this ValueTask<T[]> source) => source.Map(static r => (IEnumerable<T>)r);
}
