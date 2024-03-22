using FluentAssertions;
using FxKit.Testing.FluentAssertions;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable ConvertToLocalFunction

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     <para>
///         Tests for the monadic behavior of <see cref="Result{TOk,TErr}" />. Loosely, a monad is a container
///         that enriches a type with some effect, and provides functionality for placing values
///         in the container, as well as flat mapping them. All monads are functors.
///     </para>
///     <para>
///         The Result Monad, having two "sides", gives rise to two monads. As such, monad tests and
///         monad law tests exist for both sides.
///     </para>
///     <para>
///         This class contains tests that test the Monad Laws as they apply to <see cref="Result{TOk,TErr}" />.
///         You don't need to understand these laws in order to use <see cref="Result{TOk,TErr}" /> effectively,
///         the tests are just here to ensure we maintain a lawful implementation. Haskell is used in
///         in XML documentation to describe the laws, since it's ubiquitous within FP literature.
///     </para>
/// </summary>
/// <remarks>
///     Full "return" API tests are located in <see cref="ResultReturnTests" />. A monad must possess a
///     return operation.
/// </remarks>
public class ResultMonadTests
{
    [Test]
    public void FlatMap_FlatMapsCorrectly()
    {
        Result<int, string>.Ok(3).FlatMap(x => Result<int, string>.Ok(x + 1)).Should().BeOk(4);
        Result<int, string>.Ok(3).FlatMap(_ => Result<int, string>.Err("E")).Should().BeErr("E");
        Result<int, string>.Err("E").FlatMap(x => Result<int, string>.Ok(x + 1)).Should().BeErr("E");
    }

    [Test]
    public async Task FlatMapAsync_FlatMapsCorrectly()
    {
#pragma warning disable CS1998
        await Result<int, string>.Ok(3)
            .FlatMapAsync(async x => Result<int, string>.Ok(x + 1))
            .Should()
            .BeOk(4);

        await Result<int, string>.Ok(3)
            .FlatMapAsync(async _ => Result<int, string>.Err("E"))
            .Should()
            .BeErr("E");

        await Result<int, string>.Err("E")
            .FlatMapAsync(async x => Result<int, string>.Ok(x + 1))
            .Should()
            .BeErr("E");
#pragma warning restore CS1998
    }

    [Test]
    public void FlatMapErr_FlatMapsCorrectly()
    {
        Result<int, string>.Err("E")
            .FlatMapErr(x => Result<int, string>.Err(x + "A"))
            .Should()
            .BeErr("EA");

        Result<int, string>.Err("E").FlatMapErr(_ => Result<int, string>.Ok(4)).Should().BeOk(4);
        Result<int, string>.Ok(4).FlatMapErr(x => Result<int, string>.Err(x + "A")).Should().BeOk(4);
    }

    [Test]
    public async Task FlatMapErrAsync_FlatMapsCorrectly()
    {
#pragma warning disable CS1998
        await Result<int, string>.Err("E")
            .FlatMapErrAsync(async x => Result<int, string>.Err(x + "A"))
            .Should()
            .BeErr("EA");

        await Result<int, string>.Err("E")
            .FlatMapErrAsync(async _ => Result<int, string>.Ok(4))
            .Should()
            .BeOk(4);

        await Result<int, string>.Ok(4)
            .FlatMapErrAsync(async x => Result<int, string>.Err(x + "A"))
            .Should()
            .BeOk(4);
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
    public void LeftIdentity_ForOk_ShouldHold()
    {
        var f = (int x) => Result<int, string>.Ok(x + 1);
        Result<int, string>.Ok(3).FlatMap(f).Should().Be(f(3));
    }

    /// <summary>
    ///     Ensures that the "Left Identity" law holds, which states that lifting a value <c>a</c>
    ///     into the monadic context and flat mapping it with a function <c>f</c> is equivalent
    ///     to applying <c>f</c> onto <c>a</c>.
    ///     <example>
    ///         <b>Left Identity</b>: <c>return a >>= f ≡ f a</c>
    ///     </example>
    /// </summary>
    [Test]
    public void LeftIdentity_ForErr_ShouldHold()
    {
        var f = (string x) => Result<int, string>.Err(x + "A");
        Result<int, string>.Err("E").FlatMapErr(f).Should().Be(f("E"));
    }

    /// <summary>
    ///     Ensures that the "Right Identity" law holds, which states that flat mapping a monad
    ///     <c>m</c> with its <c>return</c> function should produce a monad unchanged from <c>m</c>.
    ///     <example>
    ///         <b>Right Identity</b>: <c>m >>= return ≡ m</c>
    ///     </example>
    /// </summary>
    [Test]
    public void RightIdentity_ForOk_ShouldHold()
    {
        Result<int, string>.Ok(4).FlatMap(Result<int, string>.Ok).Should().Be(Result<int, string>.Ok(4));
    }

    /// <summary>
    ///     Ensures that the "Right Identity" law holds, which states that flat mapping a monad
    ///     <c>m</c> with its <c>return</c> function should produce a monad unchanged from <c>m</c>.
    ///     <example>
    ///         <b>Right Identity</b>: <c>m >>= return ≡ m</c>
    ///     </example>
    /// </summary>
    [Test]
    public void RightIdentity_ForErr_ShouldHold()
    {
        Result<int, string>.Err("E")
            .FlatMapErr(Result<int, string>.Err)
            .Should()
            .Be(Result<int, string>.Err("E"));
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
    public void Associativity_ForOk_ShouldHold()
    {
        var f = (int x) => Result<int, string>.Ok(x + 1);
        var g = (int x) => Result<int, string>.Ok(x - 3);

        var lhs = Result<int, string>.Ok(4).FlatMap(f).FlatMap(g);
        var rhs = Result<int, string>.Ok(4).FlatMap(x => f(x).FlatMap(g));

        (lhs == rhs).Should().BeTrue();
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
    public void Associativity_ForErr_ShouldHold()
    {
        var f = (string x) => Result<int, string>.Err(x + "A");
        var g = (string x) => Result<int, string>.Err(x + "B");

        var lhs = Result<int, string>.Err("E").FlatMapErr(f).FlatMapErr(g);
        var rhs = Result<int, string>.Err("E").FlatMapErr(x => f(x).FlatMapErr(g));

        (lhs == rhs).Should().BeTrue();
    }

    #endregion

    #region LINQ

    [Test]
    public void SelectMany_FlatMapsCorrectly()
    {
        Result<int, string>.Ok(3).SelectMany(x => Result<int, string>.Ok(x + 1)).Should().BeOk(4);
        Result<int, string>.Ok(3).SelectMany(_ => Result<int, string>.Err("E")).Should().BeErr("E");
        Result<int, string>.Err("E").SelectMany(x => Result<int, string>.Ok(x + 1)).Should().BeErr("E");
    }

    [Test]
    public void SelectMany_WithCollectionSelector_FlatMapsCorrectly()
    {
        var okOne = Result<int, string>.Ok(3);
        var okTwo = Result<int, string>.Ok(1);
        var err = Result<int, string>.Err("E");

        var resultOne =
            from v1 in okOne
            from v2 in okTwo
            select v1 + v2;

        resultOne.Should().BeOk(4);

        var resultTwo =
            from v1 in err
            from v2 in okOne
            select v1 + v2;

        resultTwo.Should().BeErr("E");
    }

    #endregion
}
