namespace FxKit.Testing.FluentAssertions;

/// <summary>
///     Assertions for Task of <see cref="Result{T,E}" />.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TaskOptionValidationAssertions<T>
    where T : notnull
{
    private readonly Task<Option<T>> _taskOption;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="option"></param>
    public TaskOptionValidationAssertions(Task<Option<T>> option)
    {
        _taskOption = option;
    }

    /// <summary>
    ///     Asserts that the result is in the None state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task BeNone(
        string because = "",
        params object[] becauseArgs) =>
        new OptionValidationAssertions<T>(await _taskOption).BeNone(because, becauseArgs);

    /// <summary>
    ///     Asserts that the result is in the Some state.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task<T> BeSome(
        T expected,
        string because = "",
        params object[] becauseArgs) =>
        new OptionValidationAssertions<T>(await _taskOption).BeSome(
            expected,
            because,
            becauseArgs);

    /// <summary>
    ///     Asserts that the result is in the Some state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task<T> BeSome(
        string because = "",
        params object[] becauseArgs) =>
        new OptionValidationAssertions<T>(await _taskOption).BeSome(
            because,
            becauseArgs);
}
