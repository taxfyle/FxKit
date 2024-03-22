using System.Diagnostics.CodeAnalysis;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Validation
{
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

    [GenerateTransformer]
    public static TValid UnwrapOr<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        TValid fallback)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: t => t,
            Invalid: _ => fallback);

    [GenerateTransformer]
    public static IEnumerable<TInvalid> UnwrapInvalid<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source)
        where TValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: _ =>
                throw new InvalidOperationException("Cannot unwrap the error when in a valid state"),
            Invalid: e => e);

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
