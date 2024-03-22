using FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Result;

public class ResultEqualityTests
{
    [Test]
    public void Equality()
    {
        Ok(123).Should().Be(Ok(123));
        Ok(123).Should().NotBe(Ok(321));
        Ok(123).Should().NotBe(Err(123));

        Err("E").Should().Be(Err("E"));
        Err("E").Should().NotBe(Err("A"));
        Err("E").Should().NotBe(Ok("E"));

        Ok(123).Equals(Ok(123)).Should().BeTrue();
        Ok(123).Equals(Err("E")).Should().BeFalse();
        Err("E").Equals(Ok(123)).Should().BeFalse();

        // Operators
        (Ok(4) == Ok(4)).Should().BeTrue();
        (Ok(4) != Ok(4)).Should().BeFalse();
        (Err("E") == Err("E")).Should().BeTrue();
        (Err("E") != Err("E")).Should().BeFalse();
    }

    [Test]
    public void Equality_EdgeCases()
    {
        Ok(123).Should().NotBe(123);
        Err(123).Should().NotBe(123);
    }

    [Test]
    public void GetHashCode_ReturnsCorrectValuesForBothTypes()
    {
        Ok(123).GetHashCode().Should().Be(Ok(123).GetHashCode());
        Ok(123).GetHashCode().Should().NotBe(Ok(321).GetHashCode());
        Err(123).GetHashCode().Should().Be(Err(123).GetHashCode());
        Err(123).GetHashCode().Should().NotBe(Err(321).GetHashCode());
        Ok(123).GetHashCode().Should().NotBe(Err(123).GetHashCode());
    }

    /// <summary>
    ///     Baked "Ok" side constructor.
    /// </summary>
    private static Result<T, string> Ok<T>(T x)
        where T : notnull => Result<T, string>.Ok(x);

    /// <summary>
    ///     Baked "Err" side constructor.
    /// </summary>
    private static Result<int, T> Err<T>(T err)
        where T : notnull => Result<int, T>.Err(err);
}
