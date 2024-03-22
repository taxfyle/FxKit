using FxKit.CompilerServices;

namespace FxKit;

public static partial class Result
{
    #region Enumerable traversal

    /// <summary>
    ///     Maps each <typeparamref name="T" /> in the <paramref name="source" /> to
    ///     a <see cref="Result{R,E}" />.
    ///     When all of the mapped Results are in an Ok state, then the returned Result will be in an
    ///     Ok state with the mapped values as a list. Otherwise, the returned Result will be in
    ///     an Err state with the first error that was observed from the inner items.
    /// </summary>
    /// <remarks>
    ///     This is useful when you want to map a list of individual items to an overall result
    ///     where if any of the items resulted in an Err, then the overall Result would be an Err.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<IReadOnlyList<TOk>, TErr> Traverse<T, TOk, TErr>(
        this IEnumerable<T> source,
        Func<T, Result<TOk, TErr>> selector)
        where T : notnull
        where TOk : notnull
        where TErr : notnull
    {
        List<TOk>? result = null;
        foreach (var item in source)
        {
            if (!selector(item).TryGet(out var ok, out var err))
            {
                return err;
            }

            result ??= source.TryGetNonEnumeratedCount(out var count)
                ? new List<TOk>(count)
                : [];
            result.Add(ok);
        }

        return Ok<IReadOnlyList<TOk>, TErr>(result as IReadOnlyList<TOk> ?? []);
    }

    /// <summary>
    ///     The same as
    ///     <see cref="Traverse{T,TOk,TErr}" />
    ///     with <see cref="Prelude.Identity{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<IReadOnlyList<TOk>, TErr> Sequence<TOk, TErr>(
        this IEnumerable<Result<TOk, TErr>> source)
        where TOk : notnull
        where TErr : notnull
        => source.Traverse(Identity);

    #endregion
}
