using FluentAssertions;
using FxKit.Extensions;

namespace FxKit.Tests.UnitTests.Extensions;

public class ActionExtensionsTests
{
    [Test]
    public void ToUnit_ReturnsAFuncThatReturnsUnit()
    {
        var action1Called = false;
        var action2CalledWith = 0;
        Action action1 = () => action1Called = true;
        Action<int> action2 = arg => action2CalledWith = arg;

        action1.ToUnit().Invoke().Should().Be(Unit());
        action2.ToUnit().Invoke(123).Should().Be(Unit());

        action1Called.Should().BeTrue();
        action2CalledWith.Should().Be(123);
    }
}
