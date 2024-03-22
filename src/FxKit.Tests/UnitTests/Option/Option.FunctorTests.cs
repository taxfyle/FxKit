using FxKit.Testing.FluentAssertions;
using FluentAssertions;
using FxKit.Extensions;

// ReSharper disable ConvertToLocalFunction
// ReSharper disable SuggestVarOrType_Elsewhere

namespace FxKit.Tests.UnitTests.Option;

/// <summary>
///     <para>
///         Loosely, a functor is a structure you can map over.
///     </para>
///     <para>
///         This class contains tests that test the Functor Laws as they apply to <see cref="Option" />.
///         You don't need to understand these laws in order to use <see cref="Option" /> effectively,
///         the tests are just here to ensure we maintain a lawful implementation. Haskell is used in
///         in XML documentation to describe the laws, since it's ubiquitous within FP literature.
///     </para>
/// </summary>
public class OptionFunctorTests
{
    [Test]
    public void Map_MapsCorrectly()
    {
        Option<int>.Some(3).Map(x => x + 1).Should().BeSome(4);
        Option<int>.None.Map(x => x + 1).Should().BeNone();
    }

    [Test]
    public async Task MapAsync_MapsCorrectly()
    {
        await Option<int>.Some(3).MapAsync(Task.FromResult).Should().BeSome(3);
        await Option<int>.None.MapAsync(Task.FromResult).Should().BeNone();
    }

    #region Functor Laws

    /// <summary>
    ///     Ensures that the functor "Identity Law" holds, which states that mapping an identity
    ///     function over the functor should not change the functor or its value.
    ///     <example>
    ///         <b>Identity Law</b>: <c>fmap id = id</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Identity_ShouldHold()
    {
        Some(4).Map(Identity).Should().BeSome(4);
    }

    /// <summary>
    ///     Ensures that the functor "Composition Law" holds, which states that mapping functions
    ///     <c>g</c> and <c>f</c> over a functor one by one should be equivalent to mapping a
    ///     function <c>h</c> over the functor once, where <c>h</c> is the composition of
    ///     <c>f</c> and <c>g</c>.
    ///     <example>
    ///         <b>Composition</b>: <c>fmap (f . g) = fmap f . fmap g</c>
    ///     </example>
    /// </summary>
    [Test]
    public void Composition_ShouldHold()
    {
        var f = (int x) => x + 1;
        var g = (int x) => x - 3;

        var lhs = Some(4).Map(g).Map(f);
        var rhs = Some(4).Map(f.Compose(g));

        (lhs == rhs).Should().BeTrue();
    }

    #endregion

    #region LINQ

    [Test]
    public void Select_MapsCorrectly()
    {
        Option<int> source = Some(3);
        Option<int> none = None;

        source.Select(v => v + 1).Should().BeSome(4);
        none.Select(v => v + 1).Should().BeNone();
    }

    [Test]
    public void LinqSelectCompilerPattern_Matches()
    {
        var result =
            from a in Some(3)
            select a + 1;

        result.Should().BeSome(4);
    }

    #endregion
}
