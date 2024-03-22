using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

public class ResultPreludeTests
{
    [Test]
    public void Ok_CreatesAResultInTheOkState()
    {
        Ok<int, string>(4).Should().BeOk(4);
    }

    [Test]
    public void Err_CreatesAResultInTheErrState()
    {
        Err<int, string>("E").Should().BeErr("E");
    }
}
