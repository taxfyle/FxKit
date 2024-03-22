using FxKit.Testing.FluentAssertions;
using FluentAssertions;
using FxKit.Collections;

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     Traversing provides a way to rebuild a data structure and run effects along the way.
/// </summary>
public class ResultTraverseTests
{
    [Test]
    public void EnumerableTraversal_ShouldFlipTheFunctorsCorrectly()
    {
        ListOf.Many(4, 2)
            .Traverse(MustBeLessThanFive)
            .Should()
            .BeOk()
            .SequenceEqual(ListOf.Many(4, 2))
            .Should()
            .BeTrue();

        ListOf.Many(4, 6).Traverse(MustBeLessThanFive).Should().BeErr();

        Result<int, string> MustBeLessThanFive(int x) =>
            x < 5 ? x : "Error";
    }

    [Test]
    public void EnumerableSequence_ShouldFlipTheFunctorsCorrectly()
    {
        ListOf.Many(Ok(4), Ok(2))
            .Sequence()
            .Should()
            .BeOk()
            .SequenceEqual(ListOf.Many(4, 2))
            .Should()
            .BeTrue();

        ListOf.Many(Ok(4), Err("E"))
            .Sequence()
            .Should()
            .BeErr("E");

        Result<int, string> Ok(int x) => x;
        Result<int, string> Err(string x) => x;
    }
}
