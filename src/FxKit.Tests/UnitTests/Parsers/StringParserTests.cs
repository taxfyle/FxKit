using FxKit.Parsers;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Parsers;

public class StringParserTests
{
    [Test]
    public void NonNullOrWhiteSpace()
    {
        StringParser.NonNullOrWhiteSpace("").Should().BeNone();
        StringParser.NonNullOrWhiteSpace("  ").Should().BeNone();
        StringParser.NonNullOrWhiteSpace("nice").Should().BeSome("nice");
    }
}
