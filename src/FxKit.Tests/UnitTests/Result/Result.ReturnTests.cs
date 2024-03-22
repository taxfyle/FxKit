using FxKit.Testing.FluentAssertions;
using FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     "Return" in the context of Functional Programming refers to a function
///     that places a value within the monadic container.
/// </summary>
public class ResultReturnTests
{
    [Test]
    public void Ok_CreatesAResultInTheOkState()
    {
        Result<int, string>.Ok(4).Should().BeOk(4);
    }

    [Test]
    public void Both_DisallowNulls()
    {
#pragma warning disable CS8714
        FluentActions.Invoking(() => Result<int?, string>.Ok(null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        FluentActions.Invoking(() => Result<int, string?>.Err(null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();
#pragma warning restore CS8714
    }

    [Test]
    public void Err_CreatesAResultInTheErrState()
    {
        Result<int, string>.Err("E").Should().BeErr("E");
    }
}
