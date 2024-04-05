using FluentAssertions;
using FxKit.CompilerServices;
using FxKit.Testing.FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Option;

public partial class OptionFilteringTests
{
    [Test]
    public void Where_FiltersCorrectly()
    {
        Option<int> some = Some(4);
        Option<int> none = None;

        some.Where(v => v == 4).Should().Be(Some(4));
        some.Where(v => v == 5).Should().Be(None);

        none.Where(v => v == 4).Should().Be(None);
        none.Where(v => v == 5).Should().Be(None);
    }

    [Test]
    public void OfType_FiltersCorrectly()
    {
        Some(Base.One.Of()).OfType<Base.One>().Should().BeSome(new Base.One());
        Some(Base.Two.Of()).OfType<Base.Two>().Should().BeSome(new Base.Two());

        Some(Base.One.Of()).OfType<Base.Two>().Should().BeNone();
        Some(Base.Two.Of()).OfType<Base.One>().Should().BeNone();
    }
}

[Union]
internal partial record Base
{
    partial record One;

    partial record Two;
}
