using System.Diagnostics;
using FxKit.CompilerServices;

namespace FxKit;

/// <summary>
///     A structure that holds either a value in a <c>Valid</c> state or an <c>Invalid</c> state
///     with validation errors.
/// </summary>
/// <remarks>
///     Applicative behavior of this structure is implemented as harvest and coalesce failures.
/// </remarks>
/// <typeparam name="TValid"></typeparam>
/// <typeparam name="TInvalid"></typeparam>
[DebuggerDisplay("{ToString()}")]
[DebuggerStepThrough]
[Functor]
public readonly struct Validation<TValid, TInvalid>
    where TValid : notnull
    where TInvalid : notnull
{
    /// <summary>
    ///     The `Valid` value.
    /// </summary>
    private readonly TValid _value;

    /// <summary>
    ///     The collection of errors in `Invalid`.
    /// </summary>
    private readonly IEnumerable<TInvalid> _errors;

    /// <summary>
    ///     Indicates whether this structure is in a valid state.
    /// </summary>
    public bool IsValid { get; }

    private Validation(TValid valid, IEnumerable<TInvalid> errors, bool isValid)
    {
        _value = valid;
        _errors = errors;
        IsValid = isValid;
    }

    /// <summary>
    ///     Constructs a <see cref="Validation{T,E}" /> in the `Valid` state.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Validation<TValid, TInvalid> Valid(TValid value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "A Validation can't be constructed as Valid with a null value");
        }

        return new Validation<TValid, TInvalid>(
            valid: value,
            errors: Enumerable.Empty<TInvalid>(),
            isValid: true);
    }

    /// <summary>
    ///     Constructs a <see cref="Validation{T,E}" /> in the `Invalid` state.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Validation<TValid, TInvalid> Invalid(params TInvalid[] errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(
                nameof(errors),
                "A Validation can't be constructed as Invalid with a null error");
        }

        if (errors.Length == 0)
        {
            throw new InvalidOperationException("A Validation can't be constructed with no errors.");
        }

        return new Validation<TValid, TInvalid>(valid: default!, errors: errors, isValid: false);
    }

    /// <summary>
    ///     Constructs a <see cref="Validation{T,E}" /> in the `Invalid` state.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Validation<TValid, TInvalid> Invalid(IEnumerable<TInvalid> errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(
                nameof(errors),
                "A Validation can't be constructed as Invalid with a null error");
        }

        return new Validation<TValid, TInvalid>(valid: default!, errors: errors, isValid: false);
    }

    /// <summary>
    ///     Runs the `Valid` func if `Valid`, runs `Invalid` otherwise.
    /// </summary>
    public readonly R Match<R>(Func<TValid, R> Valid, Func<IEnumerable<TInvalid>, R> Invalid) =>
        IsValid ? Valid(_value) : Invalid(_errors);

    /// <inheritdoc />
    public override string ToString()
        => IsValid
            ? $"Valid({_value})"
            : $"Invalid([{string.Join(", ", _errors)}])";

    /// <inheritdoc />
    public override int GetHashCode() => Match(
        Invalid: errs => errs.GetHashCode(),
        Valid: t => t.GetHashCode());

    /// <summary>
    ///     Implicitly converts from an error to the <c>Validation</c> as an <c>Invalid</c>/
    /// </summary>
    public static implicit operator Validation<TValid, TInvalid>(TInvalid error)
        => Invalid(error);

    /// <summary>
    ///     Implicitly converts from a value to the <c>Validation</c> as a <c>Valid</c>.
    /// </summary>
    public static implicit operator Validation<TValid, TInvalid>(TValid value) => Valid(value);

    /// <summary>
    ///     Implicitly converts from a <see cref="Validation.Valid{T}" /> to a <see cref="Validation{T,E}" />
    ///     in the <c>Valid</c> state.
    /// </summary>
    public static implicit operator Validation<TValid, TInvalid>(Validation.Valid<TValid> valid) =>
        Valid(valid.Value);

    /// <summary>
    ///     Implicitly converts from a <see cref="Validation.Invalid{E}" /> to a <see cref="Validation{T,E}" />
    ///     in the <c>Invalid</c> state.
    /// </summary>
    public static implicit operator Validation<TValid, TInvalid>(Validation.Invalid<TInvalid> valid) =>
        Invalid(valid.Errors);
}

/// <summary>
///     Validation extensions.
/// </summary>
public static partial class Validation
{
    /// <summary>
    ///     Represents a Valid state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Valid<T>(T value)
        where T : notnull
    {
        /// <summary>
        ///     The valid value.
        /// </summary>
        internal readonly T Value = value;
    }

    /// <summary>
    ///     Represents an invalid value.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public struct Invalid<E>
        where E : notnull
    {
        /// <summary>
        ///     The error list.
        /// </summary>
        internal readonly IEnumerable<E> Errors;

        /// <summary>
        ///     Constructs an <see cref="Invalid{E}" /> with an error list.
        /// </summary>
        /// <param name="errors"></param>
        public Invalid(IEnumerable<E> errors)
        {
            Errors = errors;
        }

        /// <summary>
        ///     Constructs an <see cref="Invalid{E}" /> with a single error.
        /// </summary>
        /// <param name="error"></param>
        public Invalid(E error)
        {
            Errors = new[]
            {
                error
            };
        }
    }
}

/// <summary>
///     Bakes the error type into <see cref="Validation{T,E}" />..
/// </summary>
/// <remarks>
///     This exists as a way around sub-par C# type inferencing. Use it bake/encode a failure
///     type when constructing a `Valid` out out of a delegate to avoid having to specify the
///     delegate `Func` type.
/// </remarks>
/// <typeparam name="E"></typeparam>
public static class BakeErr<E>
    where E : notnull
{
    /// <summary>
    ///     Creates a <see cref="Validation{T,E}" /> in the `Valid` state.
    /// </summary>
    public static Validation<T, E> Valid<T>(T t)
        where T : notnull
        => Validation<T, E>.Valid(t);
}

/// <summary>
///     Bakes the valid type into <see cref="Validation{T,E}" />.
/// </summary>
/// <remarks>
///     This exists as a way around sub-par C# type inference. Use it bake/encode a failure
///     type when constructing a `Valid` out out of a delegate to avoid having to specify the
///     delegate `Func` type.
/// </remarks>
/// <typeparam name="T"></typeparam>
public static class BakeValid<T>
    where T : notnull
{
    /// <summary>
    ///     Creates a <see cref="Validation{T,E}" /> in the `Invalid` state.
    /// </summary>
    public static Validation<T, E> Invalid<E>(E e)
        where E : notnull
        => Validation<T, E>.Invalid(e);
    
    /// <summary>
    ///     Creates a <see cref="Validation{T,E}" /> in the `Invalid` state.
    /// </summary>
    public static Validation<T, E> Invalid<E>(IEnumerable<E> e)
        where E : notnull
        => Validation<T, E>.Invalid(e);
}
