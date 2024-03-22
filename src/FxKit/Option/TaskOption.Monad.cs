using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace FxKit;

public static partial class Option
{
    #region LINQ

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Option<U>> SelectMany<T, U>(
        this Task<Option<T>> source,
        Func<T, Task<Option<U>>> selector)
        where T : notnull
        where U : notnull
        => source.FlatMapAsyncT(selector);

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Option<UU>> SelectMany<T, U, UU>(
        this Task<Option<T>> source,
        Func<T, Task<Option<U>>> selector,
        Func<T, U, UU> project)
        where T : notnull
        where U : notnull
        where UU : notnull
        => source.Map(
                innerResult => innerResult
                    .FlatMapAsync(
                        t => selector(t)
                            .Map(targetResult => targetResult.Map(u => project(t, u)))))
            .Unwrap();

    #endregion
}
