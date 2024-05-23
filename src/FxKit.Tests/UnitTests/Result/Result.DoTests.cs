using FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     "Do" is another term for "Tap" and is typically used to perform an impure side effect
///     with the value inside the monad.
/// </summary>
public class ResultDoTests
{
    [Test]
    public void Do_ShouldWork()
    {
        var x = 2;
        var y = 3;

        Result<int, string>.Ok(2).Do(i => x += i);
        Result<int, string>.Err("Error").Do(_ => y++);

        x.Should().Be(4);
        y.Should().Be(3);
    }

    [Test]
    public void DoErr_ShouldWork()
    {
        var x = "one";
        var y = "two";

        Result<int, string>.Err("err").DoErr(e => x = e);
        Result<int, string>.Ok(3).DoErr(e => y = e);

        x.Should().Be("err");
        y.Should().Be("two");
    }

    [Test]
    public async Task DoAsync_ShouldWork()
    {
        var x = 2;
        var y = 3;

        await Result<int, string>.Ok(2)
            .DoAsync(
                async i =>
                {
                    await Task.CompletedTask;
                    x += i;
                });
        await Result<int, string>.Err("Error")
            .DoAsync(
                async _ =>
                {
                    await Task.CompletedTask;
                    y++;
                });
        x.Should().Be(4);
        y.Should().Be(3);
    }

    [Test]
    public async Task DoErrAsync_ShouldWork()
    {
        var x = "one";
        var y = "two";

        await Result<int, string>.Err("err")
            .DoErrAsync(
                async e =>
                {
                    await Task.CompletedTask;
                    x = e;
                });
        await Result<int, string>.Ok(3)
            .DoErrAsync(
                async e =>
                {
                    await Task.CompletedTask;
                    y = e;
                });
        x.Should().Be("err");
        y.Should().Be("two");
    }
}
