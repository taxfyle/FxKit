using FluentAssertions;
using FluentAssertions.Execution;

namespace FxKit.Testing.FluentAssertions;

/// <summary>
///     Assertions for <see cref="Result{T,E}" />.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="E"></typeparam>
public class ResultValidationAssertions<T, E>
    where T : notnull
    where E : notnull
{
    private readonly Result<T, E> _result;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="result"></param>
    public ResultValidationAssertions(Result<T, E> result)
    {
        _result = result;
    }

    /// <summary>
    ///     Asserts that the results match.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public void Be(
        Result<T, E> expected,
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(_result == expected)
            .FailWith("Expected results to match, but they didn't");
    }

    /// <summary>
    ///     Asserts that the result is in the Ok state.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public T BeOk(
        T expected,
        string because = "",
        params object[] becauseArgs)
    {
        var assertion = BeOk(because, becauseArgs);
        assertion.Should().Be(expected, because, becauseArgs);
        return assertion;
    }

    /// <summary>
    ///     Asserts that the result is in the Err
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public E BeErr(
        E expected,
        string because = "",
        params object[] becauseArgs)
    {
        var assertion = BeErr(because, becauseArgs);
        assertion.Should().Be(expected, because, becauseArgs);
        return assertion;
    }

    /// <summary>
    ///     Asserts that the result is in the Ok state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public T BeOk(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(_result.IsOk)
            .FailWith(
                "Expected result to be Ok{reason}, but it was {0}",
                () => _result.UnwrapErr());

        return _result.Match(
            Ok: v => v,
            Err: _ => default!);
    }

    /// <summary>
    ///     Asserts that the result is in the Err state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public E BeErr(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(!_result.IsOk)
            .FailWith(
                "Expected result to be Err{reason}, but it was {0}",
                () => _result.Unwrap());

        return _result.Match(
            Ok: _ => default!,
            Err: e => e);
    }
}
