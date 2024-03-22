using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class TaskExtensions
{
    /// <summary>
    ///     Wraps the inner value of the task in an <see cref="Option{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Option<T>> ToOptionAsync<T>(this Task<T?> source)
        where T : notnull => source.Map(Optional);

    /// <summary>
    ///     Wraps the inner value of the task in an <see cref="Option{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Option<T>> ToOptionAsync<T>(this ValueTask<T?> source)
        where T : notnull => source.Map(Optional);
}
