using FxKit.CompilerServices;

namespace FxKit;

public static partial class Validation
{
    #region Enumerable Traversal

    /// <summary>
    ///     Applicative traverse that goes from a list of normal values to
    ///     a validation of mapped normal values using a <paramref name="selector" /> which
    ///     returns a <see cref="Validation{T,E}" /> for each value in the input list.
    ///     Each input will be passed to the selector, regardless of the previous
    ///     input's validation result. If all the validations returned by <paramref name="selector" />
    ///     are <c>Valid</c>, then a <c>Valid</c> is returned with the list of mapped values.
    ///     Otherwise, returns an <c>Invalid</c> with all the validation errors collected from
    ///     the <paramref name="selector" /> calls.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Validation<IReadOnlyList<TValid>, TInvalid> Traverse<T, TValid, TInvalid>(
        this IEnumerable<T> source,
        Func<T, Validation<TValid, TInvalid>> selector)
        where T : notnull
        where TValid : notnull
        where TInvalid : notnull
    {
        // Optimize for happy path.
        var validItems = source.TryGetNonEnumeratedCount(out var count)
            ? new List<TValid>(capacity: count)
            : [];

        // Only initialize invalids when needed.
        List<TInvalid>? invalidItems = null;

        foreach (var item in source)
        {
            var projected = selector(item);
            if (projected.TryGet(out var valid, out var invalid))
            {
                validItems.Add(valid);
            }
            else
            {
                invalidItems ??= [];
                invalidItems.AddRange(invalid);
            }
        }

        return invalidItems is null
            ? Validation<IReadOnlyList<TValid>, TInvalid>.Valid(validItems)
            : Validation<IReadOnlyList<TValid>, TInvalid>.Invalid(invalidItems);
    }

    /// <summary>
    ///     The same as the applicative <see cref="Traverse{T,R, E}" /> but without the mapping step.
    ///     Essentially changes the sequence of the type stack from List of Validations to Validation of List
    ///     using the rule of List -> Validation applicative traversal.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Validation<IReadOnlyList<T>, E> Sequence<T, E>(
        this IEnumerable<Validation<T, E>> source)
        where T : notnull
        where E : notnull => Traverse(source, Identity);

    #endregion
}
