using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     Loosely (very loosely), a Natural Transformation is just a function that maps from one functor
///     to another.
/// </summary>
public class ResultNaturalTransformationTests
{
    [Test]
    public void ToValidation_ShouldWork()
    {
        Result<int, string>.Ok(4).ToValidation().Should().BeValid(4);
        Result<int, string>.Err("E").ToValidation().Should().BeInvalid(["E"]);
    }

    [Test]
    public void ToOption_ShouldWork()
    {
        Result<int, string>.Ok(4).ToOption().Should().BeSome(4);
        Result<int, string>.Err("E").ToOption().Should().BeNone();
    }
}
