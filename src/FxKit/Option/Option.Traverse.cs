using FxKit.CompilerServices;

namespace FxKit;

public static partial class Option
{
    #region Validation Traversal

    /// <summary>
    ///     Maps the value held in <c>Some</c> into a <see cref="Validation{TValid,TInvalid}" />.
    ///     If the source is <c>None</c>, then a Valid(None) is returned.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Validation<Option<TValid>, TInvalid> Traverse<T, TValid, TInvalid>(
        this Option<T> source,
        Func<T, Validation<TValid, TInvalid>> selector)
        where T : notnull
        where TValid : notnull
        where TInvalid : notnull =>
        source.TryGet(out var value)
            ? selector(value).TryGet(out var valid, out var invalid)
                ? Validation<Option<TValid>, TInvalid>.Valid(Some(valid))
                : Validation<Option<TValid>, TInvalid>.Invalid(invalid)
            : Validation<Option<TValid>, TInvalid>.Valid(Option<TValid>.None);

    /// <summary>
    ///     The same as <see cref="Traverse{T,R}" /> but without the mapping step. Essentially,
    ///     changes the sequence of the type stack from Option of Validation to Validation of Option
    ///     using the rule of Option -> Validation traversal.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Validation<Option<TValid>, TInvalid> Sequence<TValid, TInvalid>(
        this Option<Validation<TValid, TInvalid>> source)
        where TValid : notnull where TInvalid : notnull =>
        source.TryGet(out var value)
            ? value.TryGet(out var valid, out var invalid)
                ? Validation<Option<TValid>, TInvalid>.Valid(Some(valid))
                : Validation<Option<TValid>, TInvalid>.Invalid(invalid)
            : Validation<Option<TValid>, TInvalid>.Valid(Option<TValid>.None);

    #endregion

    #region Enumerable traversal

    /// <summary>
    ///     Maps each item in the <paramref name="source" /> to an <see cref="Option{T}" />.
    ///     If all the yielded <see cref="Option{T}" />s are Some, then a Some is returned
    ///     with a list of the mapped values. Otherwise, upon observing the first None, the method will
    ///     short-circuit and return None.
    /// </summary>
    /// <remarks>
    ///     This is useful when you have a list of values you want to map to Options, but you want to
    ///     end up with either a list of all Some values, or no list at all. For example, you have
    ///     a list of strings that you want to turn into a list of Guids. If ANY of the specified strings
    ///     are not valid Guids, then you would get a None value. If ALL of the specified strings are
    ///     valid Guids, then you get a Some value with the list of Guids.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Option<IReadOnlyList<R>> Traverse<T, R>(
        this IEnumerable<T> source,
        Func<T, Option<R>> selector)
        where T : notnull
        where R : notnull
    {
        List<R>? result = null;
        foreach (var item in source)
        {
            var option = selector(item);
            if (!option.TryGet(out var some))
            {
                return Option<IReadOnlyList<R>>.None;
            }

            result ??= new List<R>(source.TryGetNonEnumeratedCount(out var count) ? count : 4);
            result.Add(some);
        }

        return Optional(result as IReadOnlyList<R> ?? []);
    }

    /// <summary>
    ///     The same as <see cref="Traverse{T,R}" /> but without the mapping step. Essentially,
    ///     changes the sequence of the type stack from Option of List to List of Options
    ///     using the rule of List -> Option traversal.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Option<IReadOnlyList<T>> Sequence<T>(
        this IEnumerable<Option<T>> source)
        where T : notnull => source.Traverse(Identity);

    #endregion

    #region Result traversal

    /// <summary>
    ///     When the <paramref name="source" /> option is in the None state, then an Ok(None) is
    ///     returned immediately. Otherwise, the <paramref name="selector" /> will be called with
    ///     the value held in the Some, and the Result that was returned from <paramref name="selector" />
    ///     will have its' value put into a Some.
    /// </summary>
    /// <remarks>
    ///     This is useful when you only want to map to a Result in case the Option contains a value.
    ///     In other words, it would be Ok for the source to not contain a value.
    ///     This comes in handy when you want to parse a value into something else if and only if the
    ///     value was actually specified. You want to end up with an Option of the parsed value.
    ///     For example, filtering by workspaces for a List Transactions query; the workspace ID is
    ///     optional, but we need to map the given string to a WorkspaceId value object in case it was
    ///     specified. The inner layer can then check the Option of WorkspaceId in order to know whether
    ///     to apply the filter.
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<Option<TOk>, TErr> Traverse<T, TOk, TErr>(
        this Option<T> source,
        Func<T, Result<TOk, TErr>> selector)
        where T : notnull
        where TOk : notnull
        where TErr : notnull =>
        source.TryGet(out var value)
            ? selector(value).TryGet(out var ok, out var err)
                ? Ok<Option<TOk>, TErr>(Some(ok))
                : Err<Option<TOk>, TErr>(err)
            : Ok<Option<TOk>, TErr>(Option<TOk>.None);

    /// <summary>
    ///     Turns an Option of Result into a Result of Option using the Option to Result
    ///     traversal method.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<Option<T>, TErr> Sequence<T, TErr>(
        this Option<Result<T, TErr>> source)
        where T : notnull
        where TErr : notnull =>
        source.TryGet(out var value)
            ? value.TryGet(out var ok, out var err)
                ? Ok<Option<T>, TErr>(Some(ok))
                : Err<Option<T>, TErr>(err)
            : Ok<Option<T>, TErr>(Option<T>.None);

    #endregion
}
