using FluentAssertions;
using FxKit.Collections;
using FxKit.Testing.FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     Traversing provides a way to rebuild a data structure and run effects along the way.
/// </summary>
public class OptionTraverseTests
{
    [Test]
    public void ValidationTraversal_ShouldFlipTheFunctorsCorrectly()
    {
        Option<int>.Some(3).Traverse(IfBelowFiveAddOne).Should().BeValid(4);
        Option<int>.Some(6).Traverse(IfBelowFiveAddOne).Should().BeInvalid("Value is above maximum");
        Option<int>.None.Traverse(IfBelowFiveAddOne).Should().BeValid(None);

        Validation<int, string> IfBelowFiveAddOne(int x) =>
            x < 5 ? x + 1 : "Value is above maximum";
    }

    [Test]
    public void ValidationSequence_ShouldFlipTheFunctorsCorrectly()
    {
        Some(Validation<int, string>.Valid(4)).Sequence().Should().BeValid(4);
        Some(Validation<int, string>.Invalid("Invalid")).Sequence().Should().BeInvalid("Invalid");
        Option<Validation<int, string>>.None.Sequence().Should().BeValid(None);
    }

    [Test]
    public void EnumerableTraversal_ShouldFlipTheFunctorsCorrectly()
    {
        ListOf.Many(4, 2)
            .Traverse(MustBeLessThanFive)
            .Should()
            .BeSome()
            .SequenceEqual(ListOf.Many(4, 2))
            .Should()
            .BeTrue();

        ListOf.Many(4, 6).Traverse(MustBeLessThanFive).Should().BeNone();

        Option<int> MustBeLessThanFive(int x) =>
            x < 5 ? x : None;
    }

    [Test]
    public void EnumerableSequence_ShouldFlipTheFunctorsCorrectly()
    {
        ListOf.Many(Some(4), Some(2))
            .Sequence()
            .Should()
            .BeSome()
            .SequenceEqual(ListOf.Many(4, 2))
            .Should()
            .BeTrue();

        ListOf.Many(Some(4), Option<int>.None).Sequence().Should().BeNone();
    }

    [Test]
    public void ResultTraversal_ShouldFlipTheFunctorsCorrectly()
    {
        Option<int>.Some(3).Traverse(x => Ok<int, string>(x + 1)).Should().BeOk(4);
        Option<int>.Some(4).Traverse(_ => Err<int, string>("error")).Should().BeErr("error");
        Option<int>.None.Traverse(x => Ok<int, string>(x + 1)).Should().BeOk(None);
    }

    [Test]
    public void ResultSequence_ShouldFlipTheFunctorsCorrectly()
    {
        Some(Ok<int, string>(4)).Sequence().Should().BeOk(4);
        Some(Err<int, string>("error")).Sequence().Should().BeErr("error");
        Option<Result<int, string>>.None.Sequence().Should().BeOk(None);
    }
}
