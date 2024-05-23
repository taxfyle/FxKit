using FxKit.Parsers;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Parsers;

public class EnumParserTests
{
    [Test]
    public void Parsing()
    {
        EnumParser.Parse<MyEnum>("Foo").Should().BeValid(MyEnum.Foo);
        EnumParser.Parse<MyEnum>("Bar").Should().BeValid(MyEnum.Bar);

        EnumParser.Parse<MyEnum>("").Should().BeInvalid([EnumParseProblem.Empty]);
        EnumParser.Parse<MyEnum>("not foo").Should().BeInvalid([EnumParseProblem.Unknown]);
    }
}

file enum MyEnum
{
    Foo,
    Bar
}
