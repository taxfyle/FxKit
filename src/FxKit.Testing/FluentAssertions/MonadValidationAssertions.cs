using FluentAssertions;
using FluentAssertions.Execution;
using FxKit.Extensions;

namespace FxKit.Testing.FluentAssertions;

/// <summary>
///     Assertions for <see cref="Validation{T, E}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="E"></typeparam>
public class ValidationAssertions<T, E>
    where T : notnull
    where E : notnull
{
    private readonly Validation<T, E> _validation;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="validation"></param>
    public ValidationAssertions(Validation<T, E> validation)
    {
        _validation = validation;
    }
    
    /// <summary>
    ///     Asserts that the Validation is in the Valid state.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public T BeValid(
        T expected,
        string because = "",
        params object[] becauseArgs)
    {
        var assertion = BeValid(because, becauseArgs);
        assertion.Should().Be(expected, because, becauseArgs);
        return assertion;
    }

    /// <summary>
    ///     Asserts that the Validation is in the Invalid state.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public IReadOnlyList<E> BeInvalid(
        IEnumerable<E> expected,
        string because = "",
        params object[] becauseArgs)
    {
        var assertion = BeInvalid(because, becauseArgs);
        assertion.Should().Equal(expected, because, becauseArgs);
        return assertion;
    }

    /// <summary>
    ///     Asserts that the Validation is in the Valid state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public T BeValid(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(_validation.IsValid)
            .FailWith(
                "Expected result to be Valid{reason}, but it was {0}",
                () => _validation.UnwrapInvalid());

        return _validation.Match(
            Valid: v => v,
            Invalid: _ => default!);
    }

    /// <summary>
    ///     Asserts that the result is in the Err state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public IReadOnlyList<E> BeInvalid(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(!_validation.IsValid)
            .FailWith(
                "Expected result to be Err{reason}, but it was {0}",
                () => _validation.Unwrap());

        return _validation.Match(
            Valid: _ => default!,
            Invalid: EnumerableExtensions.ToReadOnlyList);
    }
}

/// <summary>
///     Extensions for Monad validation.
/// </summary>
public static class MonadValidationExtensions
{
    /// <summary>
    ///     Assertions for <see cref="Result{T,E}" />.
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static ResultValidationAssertions<T, E> Should<T, E>(
        this Result<T, E> result)
        where T : notnull
        where E : notnull => new(result);

    /// <summary>
    ///     Assertions for <see cref="Option{T}" />.
    /// </summary>
    /// <param name="option"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static OptionValidationAssertions<T> Should<T>(
        this Option<T> option)
        where T : notnull => new(option);

    /// <summary>
    ///     Assertions for Task of <see cref="Result{T,E}" />.
    /// </summary>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static TaskResultValidationAssertions<T, E> Should<T, E>(
        this Task<Result<T, E>> result)
        where T : notnull
        where E : notnull => new(result);

    /// <summary>
    ///     Assertions for Task of <see cref="Option{T}" />.
    /// </summary>
    /// <param name="option"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static TaskOptionValidationAssertions<T> Should<T>(
        this Task<Option<T>> option)
        where T : notnull => new(option);

    /// <summary>
    ///     Assertions for <see cref="Result{T,E}" />.
    /// </summary>
    /// <param name="validation"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static ValidationAssertions<T, E> Should<T, E>(
        this Validation<T, E> validation)
        where T : notnull
        where E : notnull => new(validation);
}
