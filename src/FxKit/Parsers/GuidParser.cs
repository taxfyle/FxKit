using FxKit.CompilerServices;

namespace FxKit.Parsers;

/// <summary>
///     Problems parsing <see cref="Guid" />.
/// </summary>
[EnumMatch]
public enum GuidParseProblem
{
    Empty,
    Malformed
}

/// <summary>
///     Parses GUIDs.
/// </summary>
public static class GuidParser
{
    /// <summary>
    ///     Parses a <see cref="Guid" /> from a <see cref="string" />.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Validation<Guid, GuidParseProblem> Parse(string? value) =>
        from nonEmptyString in StringParser.NonNullOrWhiteSpace(value)
            .ValidOr(GuidParseProblem.Empty)
        from parsed in ParseGuid(nonEmptyString)
        from nonEmptyGuid in ValidateNotEmpty(parsed)
        select nonEmptyGuid;

    /// <summary>
    ///     Parses a <see cref="Guid" /> from a <see cref="string" />.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Validation<Guid, GuidParseProblem> Parse(Guid? value) =>
        from nonEmptyGuid in ValidateNotEmpty(value)
        select nonEmptyGuid;

    /// <summary>
    ///     Parses a string into a <see cref="Guid" />.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static Validation<Guid, GuidParseProblem> ParseGuid(string value) =>
        Guid.TryParse(value, out var guid) ? guid : GuidParseProblem.Malformed;

    /// <summary>
    ///     Validates that the GUID is not <see cref="Guid.Empty" />.
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    private static Validation<Guid, GuidParseProblem> ValidateNotEmpty(Guid? guid) =>
        guid.HasValue == false || guid.Value == Guid.Empty ? GuidParseProblem.Empty : guid.Value;
}
