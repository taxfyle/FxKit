using FluentAssertions;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class UnionGeneratorCompiledTests
{
    [Test]
    public void GeneratesAWorkingUnion()
    {
        DescribeSandwich(Sandwich.Hotdog.λ()).Should().Be("a hotdog sandwich");
        DescribeSandwich(Sandwich.Sub.Of(Bread.WholeWheat.Of(NumberOfGrains: 5)))
            .Should()
            .Be("a sub with whole wheat bread with 5 grains");
        DescribeSandwich(Sandwich.Nested.Of(new Sandwich.Hotdog()))
            .Should()
            .Be("a sandwich that wraps a hotdog sandwich");
    }

    private static string DescribeSandwich(Sandwich sandwich) => sandwich.Match(
        Hotdog: _ => "a hotdog sandwich",
        Sub: sub => $"a sub with {DescribeBread(bread: sub.Bread)}",
        Nested: nested =>
            $"a sandwich that wraps {DescribeSandwich(sandwich: nested.InnerSandwich)}");

    private static string DescribeBread(Bread bread) => bread.Match(
        White: _ => "white bread",
        WholeWheat: wheat => $"whole wheat bread with {wheat.NumberOfGrains} grains");
}

[Union]
public partial record Sandwich
{
    /// <summary>
    ///     Self explanatory (at least to persons of average or higher intellect).
    ///     Life gets better when you just accept the facts.
    /// </summary>
    partial record Hotdog;

    /// <summary>
    ///     A sub, like Subway.
    /// </summary>
    /// <param name="Bread"></param>
    partial record Sub(Bread Bread);

    /// <summary>
    ///     This one sounds lethal, but pretty great.
    /// </summary>
    /// <param name="InnerSandwich"></param>
    partial record Nested(Sandwich InnerSandwich);
}

[Union]
public partial record Bread
{
    partial record White;

    partial record WholeWheat(int NumberOfGrains);
}
