using FxKit.Parsers;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Parsers;

public class NumberParserTests
{
    [Test]
    public void ParseInt()
    {
        NumberParser.ParseInt("123").Should().BeValid(123);
        NumberParser.ParseInt("123.45").Should().BeInvalid([NumberParseProblem.Malformed]);

        NumberParser.ParseInt(null)
            .Should()
            .BeInvalid([NumberParseProblem.Empty]);
    }

    [Test]
    public void ParseDecimal()
    {
        NumberParser.ParseDecimal("123").Should().BeValid(123m);
        NumberParser.ParseDecimal("123.45").Should().BeValid(123.45m);

        NumberParser.ParseDecimal("one two point bink")
            .Should()
            .BeInvalid([NumberParseProblem.Malformed]);

        NumberParser.ParseDecimal(null)
            .Should()
            .BeInvalid([NumberParseProblem.Empty]);
    }
}
