using FxKit.Testing.FluentAssertions;
namespace FxKit.Tests.UnitTests.Result;

public class TaskResultMonadTests
{
    [Test]
    public async Task SelectMany_FlatMapsCorrectly()
    {
        var ok = Result<int, string>.Ok(4).ToTask();
        var err = Result<int, string>.Err("E").ToTask();

        // Mapping to same variant.
        await ok.SelectMany(x => Result<int, string>.Ok(x + 1).ToTask()).Should().BeOk(5);
        await err.SelectMany(x => Result<int, string>.Ok(x + 1).ToTask()).Should().BeErr("E");

        // Mapping to different variant.
        await ok.SelectMany(_ => Result<int, string>.Err("E").ToTask()).Should().BeErr("E");
        await err.SelectMany(x => Result<int, string>.Ok(x + 1).ToTask()).Should().BeErr("E");
    }

    [Test]
    public async Task SelectMany_WithProjection_FlatMapsCorrectly()
    {
        var okOne = Result<int, string>.Ok(4).ToTask();
        var okTwo = Result<int, string>.Ok(4).ToTask();
        var err = Result<int, string>.Err("E").ToTask();

        var resOne =
            from a in okOne
            from b in okTwo
            select a + b;

        await resOne.Should().BeOk(8);

        var resTwo =
            from a in okOne
            from b in err
            select a + b;

        await resTwo.Should().BeErr("E");
    }

    [Test]
    public async Task SelectMany_WithProjection_WhenSelectingTask_FlatMapsCorrectly()
    {
        var okOne = Result<int, string>.Ok(4).ToTask();
        var okTwo = Result<int, string>.Ok(4).ToTask();
        var err = Result<int, string>.Err("E").ToTask();

        var resOne =
            from a in okOne
            from b in okTwo
            select (a + b).ToTask();

        await resOne.Should().BeOk(8);

        var resTwo =
            from a in okOne
            from b in err
            select (a + b).ToTask();

        await resTwo.Should().BeErr("E");
    }
}
