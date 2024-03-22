using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     Monadic API tests for a Task of Option of T.
/// </summary>
public class TaskOptionMonadTests
{
    #region LINQ

    [Test]
    public async Task SelectMany_FlatMapsCorrectly()
    {
        await Option<int>.Some(3).ToTask().SelectMany(x => Some(x + 1).ToTask()).Should().BeSome(4);
        await Option<int>.Some(3).ToTask().SelectMany(_ => Option<int>.None.ToTask()).Should().BeNone();
        await Option<int>.None.ToTask().SelectMany(x => Some(x + 1).ToTask()).Should().BeNone();
    }

    [Test]
    public async Task SelectMany_WithCollectionSelector_FlatMapsCorrectly()
    {
        var optionOne = Some(3).ToTask();
        var optionTwo = Some(1).ToTask();
        var none = Option<int>.None.ToTask();

        var resultOne =
            from v1 in optionOne
            from v2 in optionTwo
            select v1 + v2;

        await resultOne.Should().BeSome(4);

        var resultTwo =
            from v1 in none
            from v2 in optionTwo
            select v1 + v2;

        await resultTwo.Should().BeNone();
    }

    #endregion
}
