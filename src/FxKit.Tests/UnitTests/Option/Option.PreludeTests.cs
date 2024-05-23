using FluentAssertions;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Option;

public class OptionPreludeTests
{
    [Test]
    public void None_CreatesAnOptionInTheNoneState()
    {
        None.Should().Be(new FxKit.Option.None());
    }

    [Test]
    public void Some_CreatesAnOptionInTheSomeState()
    {
        Some(4).Should().BeSome(4);
    }

    [Test]
    public void Optional_CreatesAnOptionInTheCorrectState()
    {
        // Value Type
        Optional(4).Should().BeSome(4);
        Optional<int>(null).Should().BeNone();

        // Reference Type
        Optional("monads").Should().BeSome("monads");
        Optional<string>(null).Should().BeNone();
    }
}
