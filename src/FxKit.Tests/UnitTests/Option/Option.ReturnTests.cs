using FluentAssertions;
using FxKit.Testing.FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     "Return" in the context of Functional Programming refers to a function
///     that places a value within the monadic container. For the <see cref="Option" /> monad,
///     it's simply the <see cref="Prelude.Some{T}" /> function.
/// </summary>
public class OptionReturnTests
{
    [Test]
    public void Some_CreatesAnOptionInTheSomeState()
    {
        Some(4).Should().BeSome(4);
    }

    [Test]
    public void Some_DisallowsNulls()
    {
        // Disabling to test runtime behavior.
#pragma warning disable CS8714
        FluentActions.Invoking(() => Some<int?>(null!)).Should().ThrowExactly<ArgumentNullException>();
#pragma warning restore CS8714
    }

    [Test]
    public void None_CreatesAnOptionInTheNoneState()
    {
        Option<int> option = None;
        option.Should().BeNone();
    }
}
