using FluentAssertions;
using FxKit.CompilerServices;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests.Validation;

public partial class ValidationTests
{
    [Test]
    public void Match()
    {
        Validation<int, string> validation = Valid(1);
        validation.Match(
                Valid: v => v + 1,
                Invalid: _ => 0)
            .Should()
            .Be(2);

        validation = Invalid("nope");
        validation.Match(
                Valid: v => v + 1,
                Invalid: _ => 0)
            .Should()
            .Be(0);
    }

    [Test]
    public void Map()
    {
        Validation<int, string> validation = Valid(1);
        validation.Map(v => v + 1).Should().BeValid(2);

        validation = Invalid(["nope", "not good"]);
        validation.Map(v => v + 1).Should().BeInvalid(["nope", "not good"]);
    }

    [Test]
    public void FlatMap()
    {
        Validation<int, string> validation = Valid(1);

        validation.FlatMap(v => BakeErr<string>.Valid(v + 1)).Should().BeValid(2);
        validation.FlatMap(_ => BakeValid<int>.Invalid("nope")).Should().BeInvalid("nope");

        validation = Invalid(["nope", "not good"]);
        validation.FlatMap(v => BakeErr<string>.Valid(v + 1)).Should().BeInvalid(["nope", "not good"]);
        validation.FlatMap(
                _ =>
                {
                    Assert.Fail("Unreachable");
                    return BakeErr<string>.Valid(1);
                })
            .Should()
            .BeInvalid(["nope", "not good"]);
    }

    [Test]
    public void TraverseEnumerable()
    {
        int[] list = [1, 2, 3];

        list.Traverse(i => BakeErr<string>.Valid(i * 2)).Should().BeValid().Should().Equal([2, 4, 6]);

        list.Traverse(i => BakeValid<int>.Invalid(i * 2)).Should().BeInvalid().Should().Equal([2, 4, 6]);
    }

    [Test]
    public void SequenceEnumerable()
    {
        Validation<int, string>[] list = [1, 2, 3];

        list.Sequence().Should().BeValid().Should().Equal(1, 2, 3);

        list = ["bad", "worse"];
        list.Sequence().Should().BeInvalid(["bad", "worse"]);
    }

    [Test]
    public void ToOption()
    {
        Validation<int, string> validation = 1;
        validation.ToOption().Should().BeSome(1);

        validation = "invalid";
        validation.ToOption().Should().BeNone();
    }

    [Test]
    public void ToResult()
    {
        Validation<int, string> validation = 1;
        validation.ToResult().Should().BeOk(1);

        validation = Invalid(["bad", "worse"]);
        validation.ToResult().Should().BeErr().Errors.Should().Equal("bad", "worse");
    }

    [Test]
    public void Applying()
    {
        var arg1 = BakeErr<string>.Valid(1);
        var arg2 = BakeErr<string>.Valid("Valid");
        Valid(MyStructure.λ)
            .Apply(arg1)
            .Apply(arg2)
            .Should()
            .BeValid(new MyStructure(Arg1: 1, Arg2: "Valid"));

        arg1 = BakeValid<int>.Invalid("bad");
        arg2 = BakeValid<string>.Invalid("worse");
        Valid(MyStructure.λ)
            .Apply(arg1)
            .Apply(arg2)
            .Should()
            .BeInvalid(["bad", "worse"]);
    }

    [Lambda]
    public partial record MyStructure(int Arg1, string Arg2);
}
