using FxKit.Testing.FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     Loosely (very loosely), a Natural Transformation is just a function that maps from one functor
///     to another.
/// </summary>
public class OptionNaturalTransformationTests
{
    [Test]
    public void OkOrElse_ReturnsTheExpectedValue()
    {
        Option<int> some = Some(123);
        Option<int> none = None;

        some.OkOrElse(() => "Err")
            .Should()
            .BeOk(123);

        none.OkOrElse(() => "Err")
            .Should()
            .BeErr("Err");
    }

    [Test]
    public void ToResult_ReturnsTheExpectedValue()
    {
        Option<int> some = Some(123);
        Option<int> none = None;

        some.ToResult()
            .Should()
            .BeOk(123);

        none.ToResult()
            .Should()
            .BeErr(None);
    }

    [Test]
    public void ValidOr_ReturnsTheExpectedValue()
    {
        Option<int> some = Some(123);
        Option<int> none = None;

        some.ValidOr("Invalid")
            .Should()
            .BeValid(123);

        none.ValidOr("Invalid")
            .Should()
            .BeInvalid("Invalid");
    }

    [Test]
    public void ValidOrElse_ReturnsTheExpectedValue()
    {
        Option<int> some = Some(123);
        Option<int> none = None;

        some.ValidOrElse(() => "Invalid")
            .Should()
            .BeValid(123);

        none.ValidOrElse(() => "Invalid")
            .Should()
            .BeInvalid("Invalid");
    }
}
