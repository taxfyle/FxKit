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
    public static Task<Result<U, E>> SelectMany<T, U, E>(
        this Task<Result<T, E>> source,
        Func<T, Task<Result<U, E>>> selector)
        where T : notnull
        where U : notnull
        where E : notnull
        => source.FlatMapAsyncT(selector);

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
    public static Task<Result<UU, E>> SelectMany<T, U, UU, E>(
        this Task<Result<T, E>> source,
        Func<T, Task<Result<U, E>>> selector,
        Func<T, U, UU> project)
        where T : notnull
        where U : notnull
        where UU : notnull
        where E : notnull
        => source.Map(
                innerResult => innerResult
                    .FlatMapAsync(
                        t => selector(t)
                            .Map(targetResult => targetResult.Map(u => project(t, u)))))
            .Unwrap();

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
    public static Task<Result<UU, E>> SelectMany<T, U, UU, E>(
        this Task<Result<T, E>> source,
        Func<T, Task<Result<U, E>>> selector,
        Func<T, U, Task<UU>> project)
        where T : notnull
        where U : notnull
        where UU : notnull
        where E : notnull
        => source.Map(
                innerResult => innerResult
                    .FlatMapAsync(
                        t => selector(t)
                            .Map(targetResult => targetResult.MapAsync(u => project(t, u)))
                            .Unwrap()))
            .Unwrap();

    #endregion
}
