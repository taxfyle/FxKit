using FluentAssertions;

// ReSharper disable EqualExpressionComparison
#pragma warning disable CS1718
#pragma warning disable CS8602

namespace FxKit.Tests.UnitTests;

public class ValueOfTests
{
    [Test]
    public void Equality()
    {
        var guid = Guid.NewGuid();

        var value = new TestValue(guid);

        value.Should().Be(guid);
        value.Equals(guid).Should().BeTrue();
        value.Equals(value).Should().BeTrue();

        value.Equals((object?)null).Should().BeFalse();
        value.Equals((object?)new object()).Should().BeFalse();
        value.Equals((object?)value).Should().BeTrue();

        (value == value).Should().BeTrue();
        (value == guid).Should().BeTrue();
        (value != value).Should().BeFalse();
        (value != guid).Should().BeFalse();

        EqualityComparer<TestValue>.Default.Equals(value, new TestValue(guid)).Should().BeTrue();

        value.Equals(null).Should().BeFalse();
    }

    [Test]
    public void GetHashCode_ReturnsExpectedValues()
    {
        var guid = Guid.NewGuid();

        var value1 = new TestValue(guid);
        var value2 = new TestValue(guid);

        value1.GetHashCode().Should().Be(value2.GetHashCode());
        value1.GetHashCode().Should().Be(guid.GetHashCode());
    }

    private class TestValue(Guid value) : ValueOf<Guid>(value);
}
