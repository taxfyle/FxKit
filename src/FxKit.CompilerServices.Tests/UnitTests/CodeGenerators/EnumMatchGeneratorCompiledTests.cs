using FluentAssertions;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class EnumMatchGeneratorCompiledTests
{
    [Test]
    public void Match_Works()
    {
        Matched(SuperDuperEnum.One).Should().Be(1);
        Matched(SuperDuperEnum.Two).Should().Be(2);
        Matched(SuperDuperEnum.Three).Should().Be(3);
    }

    [Test]
    public void InlineMatch_Works()
    {
        InlineMatched(SuperDuperEnum.One).Should().Be(4);
        InlineMatched(SuperDuperEnum.Two).Should().Be(5);
        InlineMatched(SuperDuperEnum.Three).Should().Be(6);
    }

    private static int Matched(SuperDuperEnum e) => e.Match(
        One: () => 1,
        Two: () => 2,
        Three: () => 3);

    private static int InlineMatched(SuperDuperEnum e) => e.Match(
        One: 4,
        Two: 5,
        Three: 6);
}

[EnumMatch]
public enum SuperDuperEnum
{
    One = 1,
    Two,
    Three,
}
