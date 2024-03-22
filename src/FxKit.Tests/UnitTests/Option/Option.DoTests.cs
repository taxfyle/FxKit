using FluentAssertions;

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     "Do" is another term for "Tap" and is typically used to perform an impure side effect
///     with the value inside the monad.
/// </summary>
public class OptionDoTests
{
    [Test]
    public void Do_ShouldWork()
    {
        var x = 2;
        var y = 3;

        Option<int>.Some(2).Do(i => x += i);
        Option<int>.None.Do(_ => y++);

        x.Should().Be(4);
        y.Should().Be(3);
    }
}
