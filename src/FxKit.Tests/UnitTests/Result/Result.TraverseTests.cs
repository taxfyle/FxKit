using FluentAssertions;
using FxKit.Collections;
using FxKit.Testing.FluentAssertions;

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

    [Test]
    public void TryAggregate_ShouldAggregateValuesCorrectly()
    {
        ListOf.Many(1, 2, 3, 4)
            .TryAggregate(
                seed: 0,
                func: (acc, item) => Ok<int, string>(acc + item))
            .Should()
            .BeOk(10);
    }

    [Test]
    public void TryAggregate_ShouldReturnError_WhenConditionFails()
    {
        ListOf.Many(1, -1, 3)
            .TryAggregate(
                seed: 0,
                func: (acc, item) =>
                    item >= 0
                        ? Ok<int, string>(acc + item)
                        : Err<int, string>("Negative number encountered"))
            .Should()
            .BeErr("Negative number encountered");
    }

    [Test]
    public void TryAggregate_ShouldHandleEmptySequence()
    {
        Array.Empty<int>()
            .TryAggregate(
                seed: 0,
                func: (acc, item) => Ok<int, string>(acc + item))
            .Should()
            .BeOk(0);
    }
}
