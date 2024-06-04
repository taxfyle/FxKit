using FluentAssertions;
using FxKit.Collections;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable EqualExpressionComparison

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     "Escape" refers to exiting the monadic context. It's typically desired to remain within
///     the monadic context for as long as possible, so some of these methods should be used
///     sparingly.
/// </summary>
public class OptionEscapeTests
{
    [Test]
    public void Match_CallsTheCorrectFunc()
    {
        Option<int> some = Some(3);
        Option<int> none = None;

        some.Match(
                Some: v => v + 1,
                None: () => 10)
            .Should()
            .Be(4);

        none.Match(
                Some: v => v + 1,
                None: () => 10)
            .Should()
            .Be(10);

        FxKit.Option.Match(some, Some: v => v + 1, None: () => 10).Should().Be(4);
        FxKit.Option.Match(none, Some: v => v + 1, None: () => 10).Should().Be(10);
    }

    /// <summary>
    ///     "Nullable Value" refers to a nullable value type, as opposed to a reference type.
    /// </summary>
    [Test]
    public void ToNullableValue_ReturnsTheNullableValue()
    {
        Some(4).ToNullableValue().Should().Be(4);
        Option<int>.None.ToNullableValue().Should().BeNull();
    }

    /// <summary>
    ///     "Nullable" refers to a reference type, as opposed to a value type.
    /// </summary>
    [Test]
    public void ToNullable_ReturnsTheCorrectValue()
    {
        Some("Input").ToNullable().Should().Be("Input");
        Option<string>.None.ToNullable().Should().BeNull();
    }

    /// <summary>
    ///     "Nullable" refers to a reference type, as opposed to a value type.
    /// </summary>
    [Test]
    public async Task ToNullableAsync_ReturnsTheCorrectValue()
    {
        (await Some("Input").ToTask().ToNullableAsync()).Should().Be("Input");
        (await Option<string>.None.ToTask().ToNullableAsync()).Should().BeNull();
    }

    [Test]
    public void ToNullable_AlwaysBoxes()
    {
        Option<int>.Some(123).ToNullableValue().Should().Be(123);

        Option<int>.None.ToNullableValue().Should().Be(null);
        Option<object>.None.ToNullable().Should().Be(null);
    }

    [Test]
    public void Unwrap_ReturnsOrThrows()
    {
        Option<int>.Some(123).Unwrap().Should().Be(123);
        Option<string>.Some("woah").Unwrap().Should().Be("woah");

        FluentActions.Invoking(() => Option<string>.None.Unwrap())
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Test]
    public void UnwrapOr_ReturnsBasedOnVariant()
    {
        Option<int>.Some(123).UnwrapOr(321).Should().Be(123);
        Option<int>.None.UnwrapOr(123).Should().Be(123);
    }

    [Test]
    public void UnwrapOrElse_ReturnsBasedOnVariant()
    {
        Option<int>.Some(123).UnwrapOrElse(GetFour).Should().Be(123);
        Option<int>.None.UnwrapOrElse(GetFour).Should().Be(4);

        int GetFour() => 4;
    }

    [Test]
    public void Somes_ReturnsTheSomesInTheEnumerable()
    {
        // Mix
        ListOf.Many(Some(4), Some(3), None, Some(10), None, None, Some(2))
            .AsEnumerable()
            .Somes()
            .Should()
            .Equal(4, 3, 10, 2);

        // All Some
        ListOf.Many(Some(4), Some(5))
            .AsEnumerable()
            .Somes()
            .Should()
            .Equal(4, 5);

        // All None
        ListOf.Many(Option<int>.None, Option<int>.None)
            .AsEnumerable()
            .Somes()
            .Should()
            .BeEmpty();

        // Empty
        Enumerable.Empty<Option<int>>()
            .Somes()
            .Should()
            .BeEmpty();
    }

    [Test]
    public void Somes_ReturnsTheSomesInTheList()
    {
        // Mix
        ListOf.Many(Some(4), Some(3), None, Some(10), None, None, Some(2))
            .Somes()
            .SequenceEqual(ListOf.Many(4, 3, 10, 2))
            .Should()
            .BeTrue();

        // All Some
        ListOf.Many(Some(4), Some(5))
            .Somes()
            .SequenceEqual(ListOf.Many(4, 5))
            .Should()
            .BeTrue();

        // All None
        ListOf.Many(Option<int>.None, Option<int>.None)
            .Somes()
            .Should()
            .BeEmpty();
    }

    [Test]
    public void Somes_ShouldMaterializeWhenReadOnlyList()
    {
        ListOf.Many(Some(4), Some(3), None, Some(10), None, None, Some(2))
            .AsEnumerable()
            .Somes()
            .TryGetNonEnumeratedCount(out _)
            .Should()
            .BeFalse();

        ListOf.Many(Some(4), Some(3), None, Some(10), None, None, Some(2))
            .Somes()
            .TryGetNonEnumeratedCount(out _)
            .Should()
            .BeTrue();
    }

    [Test]
    public void SomesMap_ReturnsTheTransformedSomesInTheEnumerable()
    {
        // Mix
        ListOf.Many(
                Some("Cut To The Chase"),
                None,
                Some("Elvis Has Left The Building"),
                Some("Down And Out"),
                None,
                Some("A Fool and His Money Are Soon Parted"),
                None,
                None,
                Some("In a Pickle"))
            .AsEnumerable()
            .SomesMap(v => v.Length)
            .Should()
            .Equal(16, 27, 12, 36, 11);

        // All Some
        ListOf.Many(Some("Hello"), Some("world"))
            .AsEnumerable()
            .SomesMap(v => v.Length)
            .Should()
            .Equal(5, 5);

        // All None
        ListOf.Many(Option<string>.None, Option<string>.None)
            .AsEnumerable()
            .SomesMap(v => v.Length)
            .Should()
            .BeEmpty();

        // Empty
        Enumerable.Empty<Option<string>>()
            .SomesMap(v => v.Length)
            .Should()
            .BeEmpty();
    }

    [Test]
    public void SomesMap_ReturnsTheTransformedSomesInTheList()
    {
        // Mix
        ListOf.Many(
                Some("Cut To The Chase"),
                None,
                Some("Elvis Has Left The Building"),
                Some("Down And Out"),
                None,
                Some("A Fool and His Money Are Soon Parted"),
                None,
                None,
                Some("In a Pickle"))
            .SomesMap(v => v.Length)
            .Should()
            .Equal(16, 27, 12, 36, 11);

        // All Some
        ListOf.Many(Some("Hello"), Some("world"))
            .SomesMap(v => v.Length)
            .Should()
            .Equal(5, 5);

        // All None
        ListOf.Many(Option<string>.None, Option<string>.None)
            .SomesMap(v => v.Length)
            .Should()
            .BeEmpty();

        // Empty
        Array.Empty<Option<string>>()
            .SomesMap(v => v.Length)
            .Should()
            .BeEmpty();
    }

    [Test]
    public void SomesMap_ShouldMaterializeWhenReadOnlyList()
    {
        ListOf.Many(Some("Hello"), Some("world"))
            .AsEnumerable()
            .SomesMap(v => v.Length)
            .TryGetNonEnumeratedCount(out _)
            .Should()
            .BeFalse();

        ListOf.Many(Some("Hello"), Some("world"))
            .SomesMap(v => v.Length)
            .TryGetNonEnumeratedCount(out _)
            .Should()
            .BeTrue();
    }

    [Test]
    public void TryGet_ReturnsAndAssignsCorrectly()
    {
        Some(123).TryGet(out var value).Should().Be(true);
        value.Should().Be(123);

        Option<int> none = None;
        none.TryGet(out value).Should().BeFalse();
        value.Should().Be(default);
    }
}
