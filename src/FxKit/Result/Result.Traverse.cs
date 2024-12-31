using FxKit.CompilerServices;
using JetBrains.Annotations;

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
        [InstantHandle] Func<T, Result<TOk, TErr>> selector)
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

    /// <summary>
    /// Attempts to aggregate a sequence of values using a specified function that can fail, returning the result or an error.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulated value.</typeparam>
    /// <typeparam name="TError">The type of the error value.</typeparam>
    /// <param name="source">The sequence of elements to aggregate.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="func">A function to apply to each element and the current accumulator value, which returns a result or an error.</param>
    /// <returns>
    /// A <see cref="Result{TAccumulate, TError}"/> containing the final accumulated value if the aggregation succeeds,
    /// or an error if the aggregation fails at any step.
    /// </returns>
    /// <remarks>
    /// This method iterates over the <paramref name="source"/> sequence and applies the <paramref name="func"/> delegate
    /// to each element along with the current accumulator value. If any step in the aggregation process returns an error,
    /// the method stops processing and returns the error.
    /// </remarks>
    [GenerateTransformer]
    public static Result<TAccumulate, TError> TryAggregate<TSource, TAccumulate, TError>(
        this IEnumerable<TSource> source,
        TAccumulate seed,
        [InstantHandle] Func<TAccumulate, TSource, Result<TAccumulate, TError>> func)
        where TAccumulate : notnull
        where TError : notnull
    {
        var result = Ok<TAccumulate, TError>(seed);

        foreach (var item in source)
        {
            if (!result.TryGet(out var value, out var error))
            {
                return error;
            }

            result = func(value, item);
        }

        return result;
    }

    #endregion
}
