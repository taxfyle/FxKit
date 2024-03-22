using FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Option;

public class OptionEqualityTests
{
    [Test]
    public void Equality()
    {
        Some(123).Should().Be(Some(123));
        Some(123).Should().NotBe(Some(321));
        Some(123).Should().NotBe(None);
        Some(123).Should().NotBe(Option<int>.None);

        None.Should().Be(None);
        None.Should().Be(Option<int>.None);
        None.Should().NotBe(Some(123));
        Option<int>.None.Should().Be(None);

        // Operators
        (None == None).Should().BeTrue();
        (None != None).Should().BeFalse();

        (None == Option<int>.None).Should().BeTrue();
        (None != Option<int>.None).Should().BeFalse();
        (Option<int>.None == None).Should().BeTrue();
        (Option<int>.None != None).Should().BeFalse();

        (Some(123) == Some(123)).Should().BeTrue();
        (Option<int>.Some(123) == Some(123)).Should().BeTrue();
        (Some(123) == Option<int>.Some(123)).Should().BeTrue();
        (Option<int>.Some(123) == Option<int>.Some(123)).Should().BeTrue();

        (Option<int>.Some(321) == Option<int>.Some(123)).Should().BeFalse();
    }

    [Test]
    public void Equality_EdgeCases()
    {
        Some(123).Should().NotBe(123);
        None.Should().NotBe(123);
        Option<int>.None.Should().NotBe(123);
    }

    [Test]
    public void GetHashCode_ReturnsCorrectValuesForBothTypes()
    {
        Some(123).GetHashCode().Should().Be(Some(123).GetHashCode());
        Some(123).GetHashCode().Should().NotBe(Some(321).GetHashCode());
        Option<int>.Some(123).GetHashCode().Should().Be(Some(123).GetHashCode());
        Option<int>.Some(123).GetHashCode().Should().NotBe(Some(321).GetHashCode());

        None.GetHashCode().Should().Be(None.GetHashCode());
        Option<int>.None.GetHashCode().Should().Be(None.GetHashCode());
    }
}
