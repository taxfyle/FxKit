namespace FxKit.Parsers;

/// <summary>
///     Problems parsing numbers.
/// </summary>
public enum NumberParseProblem
{
    Empty,
    Malformed
}

/// <summary>
///     Parsers for numbers.
/// </summary>
public static class NumberParser
{
    /// <summary>
    ///     Parses an int value from a string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Validation<int, NumberParseProblem> ParseInt(string? value)
    {
        return StringParser.NonNullOrWhiteSpace(value)
            .ValidOr(NumberParseProblem.Empty)
            .FlatMap(ParseIntCore);

        static Validation<int, NumberParseProblem> ParseIntCore(string str) =>
            int.TryParse(str, out var value) ? value : NumberParseProblem.Malformed;
    }
    
    /// <summary>
    ///     Parses a decimal value from a string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Validation<decimal, NumberParseProblem> ParseDecimal(string? value)
    {
        return StringParser.NonNullOrWhiteSpace(value)
            .ValidOr(NumberParseProblem.Empty)
            .FlatMap(ParseDecimalCore);

        static Validation<decimal, NumberParseProblem> ParseDecimalCore(string str) =>
            decimal.TryParse(str, out var value) ? value : NumberParseProblem.Malformed;
    }
}
