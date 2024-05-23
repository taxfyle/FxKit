using FluentAssertions;

namespace FxKit.Tests.UnitTests.Validation;

public class ValidationDoTests
{
    [Test]
    public void Do_ShouldWork()
    {
        var x = "unchanged";
        Validation<string, string>.Valid("valid").Do(v => x = v);
        x.Should().Be("valid");

        var y = "unchanged";
        Validation<string, string>.Invalid("error").Do(_ => y = "changed");
        y.Should().Be("unchanged");
    }

    [Test]
    public void DoInvalid_ShouldWork()
    {
        var x = "unchanged";
        Validation<string, string>.Valid("valid").DoInvalid(_ => x = "changed");
        x.Should().Be("unchanged");

        var y = "unchanged";
        Validation<string, string>.Invalid("error").DoInvalid(e => y = e.First());
        y.Should().Be("error");
    }

    [Test]
    public async Task DoAsync_ShouldWork()
    {
        var x = "unchanged";
        await Validation<string, string>.Valid("valid")
            .DoAsync(
                async v =>
                {
                    await Task.CompletedTask;
                    x = v;
                });
        x.Should().Be("valid");

        var y = "unchanged";
        await Validation<string, string>.Invalid("error")
            .DoAsync(
                async _ =>
                {
                    await Task.CompletedTask;
                    y = "changed";
                });
        y.Should().Be("unchanged");
    }

    [Test]
    public async Task DoInvalidAsync_ShouldWork()
    {
        var x = "unchanged";
        await Validation<string, string>.Valid("valid")
            .DoInvalidAsync(
                async _ =>
                {
                    await Task.CompletedTask;
                    x = "changed";
                });
        x.Should().Be("unchanged");

        var y = "unchanged";
        await Validation<string, string>.Invalid("error")
            .DoInvalidAsync(
                async e =>
                {
                    await Task.CompletedTask;
                    y = e.First();
                });
        y.Should().Be("error");
    }
}
