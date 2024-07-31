using FluentAssertions;
using FxKit.Testing.FluentAssertions;

// Disabling the "no awaits in async" warning for tests.
#pragma warning disable CS1998

namespace FxKit.Tests.UnitTests.Tasks;

public class TaskExtensionsTests
{
    [Test]
    public async Task Map_Works()
    {
        (await Task.FromResult(60).Map(v => v + 9)).Should().Be(69);
        (await ValueTask.FromResult(60).Map(v => v + 9)).Should().Be(69);
    }

    [Test]
    public async Task FlatMap_Works()
    {
        (await Task.FromResult(60).FlatMap(async v => v + 9)).Should().Be(69);
        (await ValueTask.FromResult(60).FlatMap(async v => v + 9)).Should().Be(69);
    }

    [Test]
    public async Task Do_Works()
    {
        var result1 = 0;
        (await Task.FromResult(60).Do(v => result1 = v + 9)).Should().Be(60);
        result1.Should().Be(69);

        var result2 = 0;
        (await ValueTask.FromResult(60).Do(v => result2 = v + 9)).Should().Be(60);
        result2.Should().Be(69);
    }

    [Test]
    public async Task Do_NoReturnValue_Works()
    {
        var result1 = 0;
        await Task.CompletedTask.Do(() => result1 += 69);
        result1.Should().Be(69);

        var result2 = 0;
        await ValueTask.CompletedTask.Do(() => result2 += 69);
        result2.Should().Be(69);
    }

    [Test]
    public async Task Do_Async_Works()
    {
        var result1 = 0;
        (await Task.FromResult(60).DoAsync(async v => result1 = v + 9)).Should().Be(60);
        result1.Should().Be(69);

        var result2 = 0;
        (await ValueTask.FromResult(60).DoAsync(async v => result2 = v + 9)).Should().Be(60);
        result2.Should().Be(69);
    }

    [Test]
    public async Task Do_Async_NoReturnValue_Works()
    {
        var result1 = 0;
        await Task.CompletedTask.DoAsync(async () => result1 += 69);
        result1.Should().Be(69);

        var result2 = 0;
        await ValueTask.CompletedTask.DoAsync(async () => result2 += 69);
        result2.Should().Be(69);
    }

    [Test]
    public void Task_Linq()
    {
        Option<int> num1 = 1;
        Option<int> num2 = None;

        var sum =
            from n1 in num1
                .OkOrElse(() => "num1 invalid")
            from n2 in num2
                .OkOrElse(() => "num2 invalid")
            let summed = n1 + n2
            select summed.ToString();

        sum.Should().BeErr("boo! not a number");
    }
}
