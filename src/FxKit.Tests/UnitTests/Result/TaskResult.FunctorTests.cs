using FxKit.Testing.FluentAssertions;
namespace FxKit.Tests.UnitTests.Result;

public class TaskResultFunctorTests
{
    [Test]
    public async Task Select_MapsCorrectly()
    {
        var ok = Result<int, string>.Ok(4).ToTask();
        var err = Result<int, string>.Err("E").ToTask();

        await ok.Select(x => (x + 1).ToTask()).Should().BeOk(5);
        await err.Select(x => (x + 1).ToTask()).Should().BeErr("E");
    }
}
