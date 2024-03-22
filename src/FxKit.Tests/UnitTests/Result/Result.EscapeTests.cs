using FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     "Escape" refers to exiting the monadic context. It's typically desired to remain within
///     the monadic context for as long as possible, so some of these methods should be used
///     sparingly.
/// </summary>
public class ResultEscapeTests
{
    [Test]
    public void Match_CallsTheCorrectFunc()
    {
        var ok = Result<int, string>.Ok(3);
        var err = Result<int, string>.Err("Err");

        ok.Match(
                Ok: x => x + 1,
                Err: _ => 10)
            .Should()
            .Be(4);

        err.Match(
                Ok: x => x + 1,
                Err: _ => 10)
            .Should()
            .Be(10);

        FxKit.Result.Match(ok, Ok: x => x + 1, Err: _ => 10).Should().Be(4);
        FxKit.Result.Match(err, Ok: _ => "Ok", Err: Identity).Should().Be("Err");
    }

    [Test]
    public async Task MatchAsync_CallsTheCorrectFunc()
    {
        var ok = Result<int, string>.Ok(3);
        var err = Result<int, string>.Err("Err");

        var okMatchResult = await ok.MatchAsync(
            Ok: x => (x + 1).ToTask(),
            Err: _ => 10.ToTask());

        var errMatchResult = await err.MatchAsync(
            Ok: x => (x + 1).ToTask(),
            Err: _ => 10.ToTask());

        okMatchResult.Should().Be(4);
        errMatchResult.Should().Be(10);

        FxKit.Result.MatchAsync(ok, Ok: x => (x + 1).ToTask(), Err: _ => 10.ToTask())
            .Result
            .Should()
            .Be(4);

        FxKit.Result.MatchAsync(err, Ok: _ => "Ok".ToTask(), Err: x => x.ToTask())
            .Result
            .Should()
            .Be("Err");
    }

    [Test]
    public void UnwrapEither_ActsAsIdentityOnBothSides()
    {
        var ok = Result<int, int>.Ok(3);
        var err = Result<int, int>.Err(4);

        ok.UnwrapEither().Should().Be(3);
        err.UnwrapEither().Should().Be(4);
    }

    [Test]
    public void Unwrap_ReturnsTheOkValueOrThrows()
    {
        var ok = Result<int, int>.Ok(3);
        var err = Result<int, int>.Err(4);

        ok.Unwrap().Should().Be(3);

        FluentActions.Invoking(() => err.Unwrap())
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Test]
    public void UnwrapOr_ReturnsBasedOnVariant()
    {
        var ok = Result<int, int>.Ok(3);
        var err = Result<int, int>.Err(4);

        ok.UnwrapOr(0).Should().Be(3);
        err.UnwrapOr(5).Should().Be(5);
    }

    [Test]
    public void UnwrapOrElse_ReturnsBasedOnVariant()
    {
        var ok = Result<int, int>.Ok(3);
        var err = Result<int, int>.Err(4);

        ok.UnwrapOrElse(() => 0).Should().Be(3);
        err.UnwrapOrElse(() => 5).Should().Be(5);
    }

    [Test]
    public void UnwrapOrThrow_ReturnsOrThrows()
    {
        var ok = Result<int, string>.Ok(3);
        var err = Result<int, string>.Err("Error");

        ok.UnwrapOrThrow(e => new Exception(e.ToString())).Should().Be(3);

        FluentActions.Invoking(() => err.UnwrapOrThrow(e => new Exception(e.ToString())))
            .Should()
            .Throw<Exception>()
            .And.Message.Should()
            .Be("Error");
    }

    [Test]
    public void UnwrapErr_ReturnsOrThrows()
    {
        var ok = Result<int, string>.Ok(3);
        var err = Result<int, string>.Err("Error");

        err.UnwrapErr().Should().Be("Error");

        FluentActions.Invoking(() => ok.UnwrapErr())
            .Should()
            .Throw<Exception>()
            .And.Message.Should()
            .Be("3");

        FluentActions.Invoking(() => ok.UnwrapErr("Hi"))
            .Should()
            .Throw<Exception>()
            .And.Message.Should()
            .Be("Hi");
    }

    [Test]
    public void TryGet_ReturnsAndAssignsCorrectly()
    {
        Result<int, string>.Ok(123).TryGet(out var ok, out _).Should().Be(true);
        ok.Should().Be(123);

        Result<int, string>.Err("E").TryGet(out _, out var err).Should().Be(false);
        err.Should().Be("E");
    }
}
