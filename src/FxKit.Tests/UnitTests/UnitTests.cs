using FluentAssertions;

// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests;

public class UnitTests
{
    [Test]
    public void GetHashCode_IsSameForAllUnits() =>
        Unit().GetHashCode().Should().Be(Unit().GetHashCode());

    [Test]
    public void UnitPrelude_ReturnsUnit()
    {
        ReturnsUnit().Should().Be(Unit.Default);

        Unit ReturnsUnit() => Unit();
    }

    [Test]
    public void Equality()
    {
        Unit().Should().Be(Unit());
        Unit().Should().Be(Unit.Default);
        Unit().Should().Be(default(Unit));
        Unit().Should().NotBe(false);
        EqualityComparer<Unit>.Default.Equals(Unit(), Unit.Default).Should().BeTrue();

        (Unit() == Unit()).Should().BeTrue();
        (Unit() != Unit()).Should().BeFalse();
    }
}
