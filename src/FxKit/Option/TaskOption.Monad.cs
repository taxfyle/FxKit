using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace FxKit;

public static partial class Option
{
    #region LINQ

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<U>> SelectMany<T, U>(
        this Task<Option<T>> source,
        Func<T, Task<Option<U>>> selector)
        where T : notnull
        where U : notnull =>
        (await source.ConfigureAwait(false)).TryGet(out var srcValue)
            ? await selector(srcValue).ConfigureAwait(false)
            : Option<U>.None;

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<UU>> SelectMany<T, U, UU>(
        this Task<Option<T>> source,
        Func<T, Task<Option<U>>> selector,
        Func<T, U, UU> project)
        where T : notnull
        where U : notnull
        where UU : notnull =>
        (await source.ConfigureAwait(false)).TryGet(out var srcValue) &&
        (await selector(srcValue).ConfigureAwait(false)).TryGet(out var selValue)
            ? Option<UU>.Some(project(srcValue, selValue))
            : Option<UU>.None;

    #endregion
}
