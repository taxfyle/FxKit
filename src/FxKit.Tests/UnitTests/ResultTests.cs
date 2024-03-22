using FluentAssertions;

// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests;

public class ResultTests
{
    [Test]
    public void UnwrapErr_ThrowsWhenInOkState() =>
        FluentActions.Invoking(() => Result<Unit, string>.Ok(Unit()).UnwrapErr())
            .Should()
            .Throw<InvalidOperationException>();

    [Test]
    public void Ok_DisallowsNull()
    {
        // Disabling to test runtime checks.
#pragma warning disable CS8714
        FluentActions.Invoking(() => Ok<int?, Unit>(null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        FluentActions.Invoking(() => Err<Unit, int?>(null))
            .Should()
            .ThrowExactly<ArgumentNullException>();
#pragma warning restore CS8714
    }

    [Test]
    public void Unwrap_ReturnsOrThrows()
    {
        Result<int, Unit>.Ok(123).Unwrap().Should().Be(123);
        Result<string, Unit>.Ok("woah").Unwrap().Should().Be("woah");

        FluentActions.Invoking(() => Result<Unit, string>.Err("woah").Unwrap())
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Test]
    public void UnwrapOr_ReturnsBasedOnVariant()
    {
        Result<int, Unit>.Ok(123).UnwrapOr(321).Should().Be(123);
        Result<int, Unit>.Err(Unit()).UnwrapOr(123).Should().Be(123);
    }

    [Test]
    public void UnwrapErr_ReturnsTheErrValue()
    {
        var e = Err<int, string>("woah");
        e.UnwrapErr().Should().Be("woah");
    }

    [Test]
    public void Equality()
    {
        Result<int, Unit>.Ok(123).Should().Be(Result<int, Unit>.Ok(123));
        Result<Unit, int>.Err(123).Should().Be(Result<Unit, int>.Err(123));

        Result<int, Unit>.Ok(123).Should().NotBe(Result<int, Unit>.Err(Unit()));

        // Operators
        (Result<int, Unit>.Ok(123) == Result<int, Unit>.Ok(123)).Should().BeTrue();
        (Result<int, Unit>.Ok(123) != Result<int, Unit>.Ok(123)).Should().BeFalse();
    }

    [Test]
    public void Equality_EdgeCases()
    {
        Result<int, Unit>.Ok(123).Should().NotBe(123);

        Result<Unit, int>.Err(123).Should().NotBe(123);
    }

    [Test]
    public void GetHashCode_ReturnsCorrectValuesForBothTypes()
    {
        Result<int, Unit>.Ok(123).GetHashCode().Should().Be(Ok<int, Unit>(123).GetHashCode());
        Result<Unit, int>.Err(123).GetHashCode().Should().Be(Err<Unit, int>(123).GetHashCode());

        Result<int, Unit>.Ok(123).GetHashCode().Should().NotBe(Ok<int, Unit>(124).GetHashCode());
        Result<Unit, int>.Err(123).GetHashCode().Should().NotBe(Err<Unit, int>(124).GetHashCode());
    }

    [Test]
    public void IsOk_ReturnsWhetherResultIsOk()
    {
        var r = Ok<int, Unit>(123);
        r.IsOk.Should().BeTrue();

        var e = Err<int, Unit>(Unit());
        e.IsOk.Should().BeFalse();
    }

    [Test]
    public void Match_CallsTheCorrectFunc()
    {
        var ok = Result<int, Unit>.Ok(123).Match(Ok: r => -r, Err: _ => 0);
        ok.Should().Be(-123);

        var err = Result<Unit, int>.Err(123).Match(Ok: _ => 0, Err: e => -e);
        err.Should().Be(-123);
    }

    [Test]
    public void Select_MapsCorrectly()
    {
        Result<int, Unit> ok = 60;
        Result<int, Unit> err = Unit();

        var result =
            from value in ok
            select value + 9;

        result.Should().Be(Ok<int, Unit>(69));

        var errorResult =
            from value in err
            select value + 9;

        errorResult.Should().Be(Err<int, Unit>(Unit()));
    }

    [Test]
    public void SelectMany_MapsCorrectly()
    {
        Result<int, Unit> ok = 60;
        Result<int, Unit> err = Unit();

        var result = ok.SelectMany(v => Result<int, Unit>.Ok(v + 9));
        result.Should().Be(Ok<int, Unit>(69));

        var errorResult = err.SelectMany(v => Result<int, Unit>.Ok(v + 9));
        errorResult.Should().Be(Err<int, Unit>(Unit()));
    }

    [Test]
    public void SelectMany_WithBind_MapsCorrectly()
    {
        Result<int, Unit> ok1 = 60;
        Result<int, Unit> ok2 = 9;

        Result<int, Unit> err = Unit();

        var result =
            from value1 in ok1
            from value2 in ok2
            select value1 + value2;

        result.Should().Be(Ok<int, Unit>(69));

        var errorResult =
            from value1 in ok1
            from value2 in err
            select value1 + value2;

        errorResult.Should().Be(Err<int, Unit>(Unit()));
    }

    [Test]
    public void MapErr_MapsTheError()
    {
        var err = Err<int, int>(69);
        err.MapErr(e => e.ToString()).Should().Be(Err<int, string>("69"));
    }

    [Test]
    public void ToString_ReturnsExpectedRepresentation()
    {
        const string ExpectedOk = "Ok(123)";
        const string ExpectedErr = "Err(123)";

        Ok<int, Unit>(123).ToString().Should().Be(ExpectedOk);

        Err<Unit, int>(123).ToString().Should().Be(ExpectedErr);
    }

    [Test]
    public void TryGet_ReturnsAndAssignsCorrectly()
    {
        Result<int, Unit> ok = 123;
        Result<Unit, int> err = 123;

        ok.TryGet(out var ok1, out var err1).Should().BeTrue();
        ok1.Should().Be(123);
        err1.Should().Be(Unit());

        err.TryGet(out var ok2, out var err2).Should().BeFalse();
        ok2.Should().Be(Unit());
        err2.Should().Be(123);
    }

    [Test]
    public void AsOption_ReturnsTheCorrectVariant()
    {
        Result<int, Unit>.Ok(123).ToOption().Should().Be(Some(123));
        Result<int, Unit>.Err(Unit()).ToOption().Should().Be(None);
    }

    [Test]
    public void ImplicitConversions()
    {
        Result<string, int> ok = "woah";
        Result<string, int> err = 123;

        ok.Should().Be(Ok<string, int>("woah"));
        err.Should().Be(Err<string, int>(123));
    }
}
