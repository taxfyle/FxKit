using FluentAssertions;
using FxKit.Testing.FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable ConvertToLocalFunction

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     <para>
///         Tests for the monadic behavior of <see cref="Option{T}" />. Loosely, a monad is a container
///         that enriches a type with some effect, and provides functionality for placing values
///         in the container, as well as flat mapping them. All monads are functors.
///     </para>
///     <para>
///         This class contains tests that test the Monad Laws as they apply to <see cref="Option{T}" />.
///         You don't need to understand these laws in order to use <see cref="Option{T}" /> effectively,
///         the tests are just here to ensure we maintain a lawful implementation. Haskell is used in
///         in XML documentation to describe the laws, since it's ubiquitous within FP literature.
///     </para>
/// </summary>
/// <remarks>
///     Full "return" API tests are located in <see cref="OptionReturnTests" />. A monad must possess a
///     return operation.
/// </remarks>
public class OptionMonadTests
{
    [Test]
    public void FlatMap_FlatMapsCorrectly()
    {
        Option<int>.Some(3).FlatMap(x => Some(x + 1)).Should().BeSome(4);
        Option<int>.Some(3).FlatMap(_ => Option<int>.None).Should().BeNone();
        Option<int>.None.FlatMap(x => Some(x + 1)).Should().BeNone();
    }

    [Test]
    public async Task FlatMapAsync_FlatMapsCorrectly()
    {
#pragma warning disable CS1998
        await Option<int>.Some(3).FlatMapAsync(async x => Some(x + 1)).Should().BeSome(4);
        await Option<int>.Some(3).FlatMapAsync(async _ => Option<int>.None).Should().BeNone();
        await Option<int>.None.FlatMapAsync(async x => Some(x + 1)).Should().BeNone();
#pragma warning restore CS1998
    }

    #region Monad Laws

    /// <summary>
    ///     Ensures that the "Left Identity" law holds, which states that lifting a value <c>a</c>
    ///     into the monadic context and flat mapping it with a function <c>f</c> is equivalent
    ///     to applying <c>f</c> onto <c>a</c>.
    ///     <example>
    ///         <b>Left Identity</b>: <c>return a >>= f ≡ f a</c>
    ///     </example>
    /// </summary>
    [Test]
    public void LeftIdentity_ShouldHold()
    {
        var f = (int x) => Some(x + 1);
        Some(3).FlatMap(f).Should().Be(f(3));
    }

    /// <summary>
    ///     Ensures that the "Right Identity" law holds, which states that flat mapping a monad
    ///     <c>m</c> with its <c>return</c> function should produce a monad unchanged from <c>m</c>.
    ///     <example>
    ///         <b>Right Identity</b>: <c>m >>= return ≡ m</c>
    ///     </example>
    /// </summary>
    [Test]
    public void RightIdentity_ShouldHold()
    {
        Some(4).FlatMap(Some).Should().Be(Some(4));
    }

    /// <summary>
    ///     Ensures that the "Associativity" law holds, which states that under monadic composition,
    ///     the nesting/grouping of operations is irrelevant, in the same way that
    ///     <c>(1 + 2) + 3 ≡ 1 + (2 + 3)</c>.
    ///     <example>
    ///         <b>Associativity</b>: <c>(m >>= f) >>= g ≡ m >>= (\x -> f x >>= g)</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Associativity_ShouldHold()
    {
        var f = (int x) => Some(x + 1);
        var g = (int x) => Some(x - 3);

        var lhs = Some(4).FlatMap(f).FlatMap(g);
        var rhs = Some(4).FlatMap(x => f(x).FlatMap(g));

        (lhs == rhs).Should().BeTrue();
    }

    #endregion

    [Test]
    public void Or_ShouldBeReturned()
    {
        var l = Option<int>.None;
        var r = l.Or(1);
        var rElse = l.OrElse(() => 2);

        r.Should().BeSome(1);
        rElse.Should().BeSome(2);
    }

    #region LINQ

    [Test]
    public void SelectMany_FlatMapsCorrectly()
    {
        Option<int>.Some(3).SelectMany(x => Some(x + 1)).Should().BeSome(4);
        Option<int>.Some(3).SelectMany(_ => Option<int>.None).Should().BeNone();
        Option<int>.None.SelectMany(x => Some(x + 1)).Should().BeNone();
    }

    [Test]
    public void SelectMany_WithCollectionSelector_FlatMapsCorrectly()
    {
        Option<int> optionOne = Some(3);
        Option<int> optionTwo = Some(1);
        Option<int> none = None;

        var resultOne =
            from v1 in optionOne
            from v2 in optionTwo
            select v1 + v2;

        resultOne.Should().BeSome(4);

        var resultTwo =
            from v1 in none
            from v2 in optionTwo
            select v1 + v2;

        resultTwo.Should().BeNone();
    }

    #endregion
}
