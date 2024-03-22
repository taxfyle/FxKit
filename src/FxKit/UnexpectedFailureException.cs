namespace FxKit;

/// <summary>
///     Thrown when something failed that we were not expecting to fail under normal circumstances.
/// </summary>
public class UnexpectedFailureException : Exception
{
    /// <inheritdoc />
    public UnexpectedFailureException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public UnexpectedFailureException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
