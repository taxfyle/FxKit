namespace FxKit.Parsers;

/// <summary>
///     Problems parsing enums.
/// </summary>
public enum EnumParseProblem
{
    Empty,
    Unknown
}

/// <summary>
///     Enum parser.
/// </summary>
public static class EnumParser
{
    /// <summary>
    ///     Parses an enum of the given type.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns></returns>
    public static Validation<TEnum, EnumParseProblem> Parse<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        return StringParser.NonNullOrWhiteSpace(value)
            .ValidOr(EnumParseProblem.Empty)
            .FlatMap(ParseEnumCore);

        static Validation<TEnum, EnumParseProblem> ParseEnumCore(string str) =>
            Enum.TryParse<TEnum>(str, out var parsed) ? parsed : EnumParseProblem.Unknown;
    }
}
