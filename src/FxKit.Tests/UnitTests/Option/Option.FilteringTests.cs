using FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Option;

public class OptionFilteringTests
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
}
