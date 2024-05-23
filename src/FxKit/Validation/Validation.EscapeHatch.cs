using System.Diagnostics.CodeAnalysis;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Validation
{
    /// <summary>
    ///     Unwraps the value from the <see cref="Validation{TValid,TInvalid}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="exceptionMessage"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the value is in an Invalid state.
    /// </exception>
    [GenerateTransformer]
    public static TValid Unwrap<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        string? exceptionMessage = null)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: t => t,
            Invalid: _ =>
                throw new InvalidOperationException(
                    exceptionMessage ?? "Cannot unwrap the the value when in an Invalid state."));

    /// <summary>
    ///     Unwraps the value from the <see cref="Validation{TValid,TInvalid}" />, falling back to the
    ///     <paramref name="fallback" /> if in an Invalid state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fallback"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static TValid UnwrapOr<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        TValid fallback)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: t => t,
            Invalid: _ => fallback);

    /// <summary>
    ///     Unwraps the errors from the <see cref="Validation{TValid,TInvalid}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the value is in a Valid state.
    /// </exception>
    [GenerateTransformer]
    public static IEnumerable<TInvalid> UnwrapInvalid<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: _ =>
                throw new InvalidOperationException("Cannot unwrap the error when in a valid state"),
            Invalid: e => e);

    /// <summary>
    ///     Returns <c>true</c> if the <see cref="Validation{TValid,TInvalid}" /> is in a Valid state
    ///     and assigns the value to <paramref name="valid" />; returns <c>false</c> otherwise and
    ///     assigns the errors to <paramref name="invalid" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="valid"></param>
    /// <param name="invalid"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    public static bool TryGet<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        [NotNullWhen(true)] out TValid? valid,
        [NotNullWhen(false)] out IEnumerable<TInvalid>? invalid)
        where TValid : notnull
        where TInvalid : notnull
    {
        var isValid = source.IsValid;
        valid = isValid ? source.Unwrap() : default;
        invalid = isValid ? default : source.UnwrapInvalid();
        return isValid;
    }
}
