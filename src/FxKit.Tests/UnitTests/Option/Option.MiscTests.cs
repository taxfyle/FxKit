using FxKit.Testing.FluentAssertions;
using FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Option;

public class OptionMiscTests
{
    [Test]
    public void ToString_ReturnsTheExpectedValue()
    {
        Option<int>.None.ToString().Should().Be("None");
        None.ToString().Should().Be("None");

        Option<int>.Some(123).ToString().Should().Be("Some(123)");
        Some(123).ToString().Should().Be("Some(123)");
    }

    [Test]
    public void ImplicitConversionToSome()
    {
        Option<int> some = 123;
        some.Should().BeSome(123);
    }

    [Test]
    public void ImplicitConversionToNone()
    {
        Option<string> none = null;
        none.Should().BeNone();
    }
}
