using FxKit.Parsers;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Parsers;

public class GuidParserTests
{
    [Test]
    public void ParseGuid()
    {
        GuidParser.Parse("8af4d4f5-6e53-414c-b00f-456faa8cb12e")
            .Should()
            .BeValid(Guid.Parse("8af4d4f5-6e53-414c-b00f-456faa8cb12e"));

        GuidParser.Parse(Guid.Parse("8af4d4f5-6e53-414c-b00f-456faa8cb12e"))
            .Should()
            .BeValid(Guid.Parse("8af4d4f5-6e53-414c-b00f-456faa8cb12e"));

        GuidParser.Parse("not a guid")
            .Should()
            .BeInvalid([GuidParseProblem.Malformed]);
        
        GuidParser.Parse("")
            .Should()
            .BeInvalid([GuidParseProblem.Empty]);
        
        GuidParser.Parse((Guid?)null)
            .Should()
            .BeInvalid([GuidParseProblem.Empty]);
        
        GuidParser.Parse(Guid.Empty)
            .Should()
            .BeInvalid([GuidParseProblem.Empty]);
    }
}
