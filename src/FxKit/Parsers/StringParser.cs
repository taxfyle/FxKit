namespace FxKit.Parsers;

/// <summary>
///     Parses <see cref="string" /> to other types.
/// </summary>
public static class StringParser
{
    /// <summary>
    ///     Returns an <see cref="Option" /> that will have a Some value of a valid string that isn't
    ///     null or white space.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Option<string> NonNullOrWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? None : Some(value);
}
