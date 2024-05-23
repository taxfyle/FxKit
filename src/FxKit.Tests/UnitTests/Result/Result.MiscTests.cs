using FluentAssertions;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

public class ResultMiscTests
{
    [Test]
    public void ToString_ReturnsTheExpectedValue()
    {
        Result<int, string>.Ok(4).ToString().Should().Be("Ok(4)");
        Result<int, string>.Err("E").ToString().Should().Be("Err(E)");
    }

    [Test]
    public void RequireTrue_ShouldReturnOk_WhenTrue()
    {
        bool? nullableTrue = true;
        FxKit.Result.RequireTrue(nullableTrue).Should().BeOk(Unit());
        // ReSharper disable once EqualExpressionComparison
        FxKit.Result.RequireTrue(4 == 4).Should().BeOk(Unit());

        bool? nullable = null;
        FxKit.Result.RequireTrue(nullable).Should().BeErr(Unit());
        // ReSharper disable once EqualExpressionComparison
        FxKit.Result.RequireTrue(4 != 4).Should().BeErr(Unit());
    }

    [Test]
    public void ImplicitConversion_ToOk()
    {
        Result<int, string> ok = 4;
        ok.Should().BeOk(4);
    }

    [Test]
    public void ImplicitConversion_ToErr()
    {
        Result<int, string> err = "E";
        err.Should().BeErr("E");
    }
}
