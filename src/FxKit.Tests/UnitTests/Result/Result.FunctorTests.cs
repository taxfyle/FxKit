using FluentAssertions;
using FxKit.Extensions;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Result;

/// <summary>
///     <para>
///         Loosely, a functor is a structure you can map over.
///     </para>
///     <para>
///         The Result Functor, having two "sides" you can map over, is a bifunctor, and gives rise to
///         two functors (and two monads). As such, functor tests and functor law tests exist for both sides.
///     </para>
///     <para>
///         This class contains tests that test the Functor Laws as they apply to <see cref="Result{TOk,TErr}" />.
///         You don't need to understand these laws in order to use <see cref="Result{TOk,TErr}" /> effectively,
///         the tests are just here to ensure we maintain a lawful implementation. Haskell is used in
///         in XML documentation to describe the laws, since it's ubiquitous within FP literature.
///     </para>
/// </summary>
public class ResultFunctorTests
{
    [Test]
    public void Map_MapsCorrectly()
    {
        Result<int, string>.Ok(3).Map(x => x + 1).Should().BeOk(4);
        Result<int, string>.Err("E").Map(x => x + 1).Should().BeErr("E");
    }

    [Test]
    public void MapAsync_MapsCorrectly()
    {
        Result<int, string>.Ok(3).MapAsync(x => (x + 1).ToTask()).Result.Should().BeOk(4);
        Result<int, string>.Err("E").MapAsync(x => (x + 1).ToTask()).Result.Should().BeErr("E");
    }

    [Test]
    public void MapErr_MapsCorrectly()
    {
        Result<int, string>.Ok(3).MapErr(x => x + "A").Should().BeOk(3);
        Result<int, string>.Err("E").MapErr(x => x + "A").Should().BeErr("EA");
    }

    [Test]
    public void MapErrAsync_MapsCorrectly()
    {
        Result<int, string>.Ok(3).MapErrAsync(x => (x + "A").ToTask()).Result.Should().BeOk(3);
        Result<int, string>.Err("E").MapErrAsync(x => (x + "A").ToTask()).Result.Should().BeErr("EA");
    }

    #region Functor Laws

    /// <summary>
    ///     Ensures that the functor "Identity Law" holds for the Ok side, which states that mapping an
    ///     identity function over the functor should not change the functor or its value.
    ///     <example>
    ///         <b>Identity Law</b>: <c>fmap id = id</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Identity_ForOk_ShouldHold()
    {
        Result<int, string>.Ok(4).Map(Identity).Should().BeOk(4);
    }

    /// <summary>
    ///     Ensures that the functor "Identity Law" holds for the Err side, which states that mapping an
    ///     identity function over the functor should not change the functor or its value.
    ///     <example>
    ///         <b>Identity Law</b>: <c>fmap id = id</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Identity_ForErr_ShouldHold()
    {
        Result<int, string>.Err("E").MapErr(Identity).Should().BeErr("E");
    }

    /// <summary>
    ///     Ensures that the functor "Composition Law" holds for the Ok side, which states that mapping
    ///     functions <c>g</c> and <c>f</c> over a functor one by one should be equivalent to mapping a
    ///     function <c>h</c> over the functor once, where <c>h</c> is the composition of
    ///     <c>f</c> and <c>g</c>.
    ///     <example>
    ///         <b>Composition</b>: <c>fmap (f . g) = fmap f . fmap g</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Composition_ForOk_ShouldHold()
    {
        var f = (int x) => x + 1;
        var g = (int x) => x + 3;

        var lhs = Result<int, string>.Ok(4).Map(g).Map(f);
        var rhs = Result<int, string>.Ok(4).Map(f.Compose(g));

        (lhs == rhs).Should().BeTrue();
    }

    /// <summary>
    ///     Ensures that the functor "Composition Law" holds for the Err side, which states that mapping
    ///     functions <c>g</c> and <c>f</c> over a functor one by one should be equivalent to mapping a
    ///     function <c>h</c> over the functor once, where <c>h</c> is the composition of
    ///     <c>f</c> and <c>g</c>.
    ///     <example>
    ///         <b>Composition</b>: <c>fmap (f . g) = fmap f . fmap g</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Composition_ForErr_ShouldHold()
    {
        var f = (string x) => x + "A";
        var g = (string x) => x + "B";

        var lhs = Result<int, string>.Err("E").MapErr(g).MapErr(f);
        var rhs = Result<int, string>.Err("E").MapErr(f.Compose(g));

        (lhs == rhs).Should().BeTrue();
    }

    #endregion

    #region LINQ

    [Test]
    public void Select_MapsCorrectly()
    {
        Result<int, string>.Ok(3).Select(x => x + 1).Should().BeOk(4);
        Result<int, string>.Err("E").Select(x => x + 1).Should().BeErr("E");
    }

    [Test]
    public void LinqSelectCompilerPattern_Matches()
    {
        var result =
            from a in Result<int, string>.Ok(3)
            select a + 1;

        result.Should().BeOk(4);
    }

    #endregion
}
