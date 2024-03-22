using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class TaskExtensions
{
    /// <summary>
    ///     Wraps <see cref="T" /> within a completed <see cref="Task{TResult}" />.
    /// </summary>
    /// <param name="t"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> ToTask<T>(this T t) => Task.FromResult(t);
}
