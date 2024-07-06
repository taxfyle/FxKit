// ReSharper disable InconsistentNaming

namespace FxKit;

public static partial class Result
{
    #region LINQ

    /// <summary>
    ///     LINQ Extension Method for `FlatMapAsyncT`.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static async Task<Result<U, E>> SelectMany<T, U, E>(
        this Task<Result<T, E>> source,
        Func<T, Task<Result<U, E>>> selector)
        where T : notnull
        where U : notnull
        where E : notnull
        => (await source.ConfigureAwait(false)).TryGet(out var ok, out var err)
            ? await selector(ok).ConfigureAwait(false)
            : Result<U, E>.Err(err);

    /// <summary>
    ///     LINQ Extension Method for `FlatMapAsyncT`.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="project"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="UU"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static async Task<Result<UU, E>> SelectMany<T, U, UU, E>(
        this Task<Result<T, E>> source,
        Func<T, Task<Result<U, E>>> selector,
        Func<T, U, UU> project)
        where T : notnull
        where U : notnull
        where UU : notnull
        where E : notnull
        => (await source.ConfigureAwait(false)).TryGet(out var srcOk, out var srcErr)
            ? (await selector(srcOk).ConfigureAwait(false)).TryGet(out var selOk, out var selErr)
                ? project(srcOk, selOk)
                : Result<UU, E>.Err(selErr)
            : Result<UU, E>.Err(srcErr);

    /// <summary>
    ///     LINQ Extension Method for `FlatMapAsyncT`.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="project"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="UU"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static async Task<Result<UU, E>> SelectMany<T, U, UU, E>(
        this Task<Result<T, E>> source,
        Func<T, Task<Result<U, E>>> selector,
        Func<T, U, Task<UU>> project)
        where T : notnull
        where U : notnull
        where UU : notnull
        where E : notnull
        => (await source.ConfigureAwait(false)).TryGet(out var srcOk, out var srcErr)
            ? (await selector(srcOk).ConfigureAwait(false)).TryGet(out var selOk, out var selErr)
                ? await project(srcOk, selOk).ConfigureAwait(false)
                : Result<UU, E>.Err(selErr)
            : Result<UU, E>.Err(srcErr);

    #endregion
}
