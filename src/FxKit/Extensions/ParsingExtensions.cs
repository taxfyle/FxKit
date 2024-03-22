namespace FxKit.Extensions;

/// <summary>
///     Helpers for parsing values in code paths where a parse error would be considered
///     exceptional.
/// </summary>
public static class ParsingExtensions
{
    /// <summary>
    ///     Unwraps the <paramref name="validation" />; if unable to,
    ///     collects all the errors and throws a <see cref="UnexpectedFailureException" />.
    /// </summary>
    /// <remarks>
    ///     This should only be used for parsing raw values into objects
    ///     that really shouldn't fail.
    /// </remarks>
    /// <param name="validation">The validation result.</param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static T UnwrapOrThrowParseException<T, E>(
        this Validation<T, E> validation)
        where T : notnull
        where E : notnull
    {
        if (validation.TryGet(out var result, out var errors))
        {
            return result;
        }

        var joined = string.Join("", errors.Select(e => e.ToString()).WhereNotNull());
        throw new UnexpectedFailureException(MakeMessage<T>(joined));
    }

    /// <summary>
    ///     Makes a message for the exception.
    /// </summary>
    /// <param name="failureMessage"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static string MakeMessage<T>(string failureMessage)
        where T : notnull =>
        $"Parsing raw values into a {typeof(T).Name} failed: {failureMessage}";
}
