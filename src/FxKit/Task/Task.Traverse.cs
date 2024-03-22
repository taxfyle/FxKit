using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

/// <summary>
///     Task traversal.
/// </summary>
public static partial class TaskExtensions
{
    /// <summary>
    ///     Applicative traverse from an <see cref="IEnumerable{T}" /> to a
    ///     <see cref="IReadOnlyList{R}" />. It is essentially doing a `Task.WhenAll` on a
    ///     list that is mapped to a list of tasks.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IReadOnlyList<R>> Traverse<T, R>(
        this IEnumerable<T> source,
        Func<T, Task<R>> selector)
    {
        var tasks = source.Select(selector);
        return await Task.WhenAll(tasks);
    }
}
