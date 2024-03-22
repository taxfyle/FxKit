using FluentAssertions;
using FluentAssertions.Execution;

namespace FxKit.Testing.FluentAssertions;

/// <summary>
///     Assertions for <see cref="Result{T,E}" />.
/// </summary>
/// <typeparam name="T"></typeparam>
public class OptionValidationAssertions<T>
    where T : notnull
{
    private readonly Option<T> _option;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="option"></param>
    public OptionValidationAssertions(Option<T> option)
    {
        _option = option;
    }

    /// <summary>
    ///     Asserts that the result is in the None state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public void BeNone(
        string because = "",
        params object[] becauseArgs) =>
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(!_option.IsSome)
            .FailWith(
                "Expected result to be None, but it was {0}",
                () => _option);

    /// <summary>
    ///     Asserts that the options match.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public void Be(
        Option<T> expected,
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(_option == expected)
            .FailWith("Expected options to match, but they didn't");
    }

    /// <summary>
    ///     Asserts that the result is in the Some state.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public T BeSome(
        T expected,
        string because = "",
        params object[] becauseArgs)
    {
        var assertion = BeSome(because, becauseArgs);
        assertion.Should().Be(expected, because, becauseArgs);
        return assertion;
    }

    /// <summary>
    ///     Asserts that the result is in the Some state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public T BeSome(
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(_option.IsSome)
            .FailWith("Expected result to be Ok{reason}, but it was None");

        return _option.Match(
            Some: v => v,
            None: () => default!);
    }
}
