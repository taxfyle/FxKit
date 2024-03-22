namespace FxKit.Testing.FluentAssertions;

/// <summary>
///     Assertions for Task of <see cref="Result{T,E}" />.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="E"></typeparam>
public class TaskResultValidationAssertions<T, E>
    where T : notnull
    where E : notnull
{
    private readonly Task<Result<T, E>> _taskResult;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="result"></param>
    public TaskResultValidationAssertions(Task<Result<T, E>> result)
    {
        _taskResult = result;
    }

    /// <summary>
    ///     Asserts that the result is in the Ok state.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task<T> BeOk(
        T expected,
        string because = "",
        params object[] becauseArgs) =>
        new ResultValidationAssertions<T, E>(await _taskResult).BeOk(
            expected,
            because,
            becauseArgs);

    /// <summary>
    ///     Asserts that the result is in the Err
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task<E> BeErr(
        E expected,
        string because = "",
        params object[] becauseArgs) =>
        new ResultValidationAssertions<T, E>(await _taskResult).BeErr(
            expected,
            because,
            becauseArgs);

    /// <summary>
    ///     Asserts that the result is in the Ok state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task<T> BeOk(
        string because = "",
        params object[] becauseArgs) =>
        new ResultValidationAssertions<T, E>(await _taskResult).BeOk(
            because,
            becauseArgs);

    /// <summary>
    ///     Asserts that the result is in the Err state.
    /// </summary>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public async Task<E> BeErr(
        string because = "",
        params object[] becauseArgs) =>
        new ResultValidationAssertions<T, E>(await _taskResult).BeErr(
            because,
            becauseArgs);
}
