using System.Diagnostics;

namespace FxKit;

/// <summary>
///     Result factories in the prelude.
/// </summary>
[DebuggerStepThrough]
public static partial class Prelude
{
    /// <summary>
    ///     Creates a <see cref="Result{T,E}" /> in the Ok state.
    /// </summary>
    /// <param name="ok"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static Result<T, E> Ok<T, E>(T ok)
        where T : notnull
        where E : notnull
        => Result<T, E>.Ok(ok);

    /// <summary>
    ///     Creates a <see cref="Result{T,E}" /> in the Error state.
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="E"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Result<T, E> Err<T, E>(E error)
        where T : notnull
        where E : notnull => Result<T, E>.Err(error);
}
