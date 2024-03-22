using System.Collections.Immutable;
using FxKit.CompilerServices;
using FxKit.Extensions;

namespace FxKit;

/// <summary>
///     Represents a Validation Error for use in a <see cref="Result{T,E}" />.
/// </summary>
/// <param name="Errors"></param>
/// <typeparam name="E"></typeparam>
public record ValidationError<E>(ImmutableList<E> Errors)
    where E : notnull
{
    /// <inheritdoc />
    public override string ToString() => ToJoinedString();

    /// <summary>
    ///     Returns a string of the stringified errors joined by the given separator (default: ". ")
    /// </summary>
    /// <returns></returns>
    public string ToJoinedString(string separator = ". ") => string.Join(
        separator,
        Errors.Select(e => e.ToString()).WhereNotNull());
}

public static partial class Validation
{
    /// <summary>
    ///     Converts the Validation to an Option in the Some state if it was Valid.
    /// </summary>
    /// <remarks>
    ///     Using this transformation will make you lose the failure types.
    /// </remarks>
    /// <param name="source"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Option<TValid> ToOption<TValid, TInvalid>(this Validation<TValid, TInvalid> source)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: Option<TValid>.Some,
            Invalid: _ => Option<TValid>.None);

    /// <summary>
    ///     Converts the Validation to a Result in the Ok state if it was Valid, and in the Err
    ///     state with a <see cref="ValidationError{E}" /> containing the list of failures if it
    ///     was invalid.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<TValid, ValidationError<TInvalid>> ToResult<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: Result<TValid, ValidationError<TInvalid>>.Ok,
            Invalid: errs => errs.Aggregate(
                new ValidationError<TInvalid>(Enumerable.Empty<TInvalid>().ToImmutableList()),
                (ve, e) => new ValidationError<TInvalid>(Errors: ve.Errors.Add(e))));

    /// <summary>
    ///     Converts the Validation to a Result in the Ok state if it was Valid, and in the Err
    ///     state with an Error created by <see cref="selector" /> if it was invalid.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <typeparam name="TNewInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Result<TValid, TNewInvalid> OkOrElse<TValid, TInvalid, TNewInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<IReadOnlyList<TInvalid>, TNewInvalid> selector)
        where TValid : notnull
        where TInvalid : notnull
        where TNewInvalid : notnull
        => source.ToResult().MapErr(e => selector(e.Errors));
}
