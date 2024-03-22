using FluentAssertions;
using FxKit.CompilerServices.CodeGenerators;
using FxKit.CompilerServices.Tests.TestUtils;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class UnionGeneratorTests
{
    [Test]
    public async Task GeneratesUnion()
    {
        var output = Generate(
            @"
using System.Collections.Generic;
using FxKit.CompilerServices;

namespace Super.Duper.Unions;

[Union]
public partial record CSharpLanguageIssues
{
    partial record TerribleLambdaInference;
    partial record NoExhaustiveMatch(List<string> PotentialSolutions);
    partial record NoHigherKindedTypes(List<string> PotentialSolutions, bool CanBeDebated);
    partial record NoDiscriminatedUnions();
}
");
        await output.VerifyGeneratedCode();
    }

    [Test]
    public void DoesNotGenerateWhenConditionsAreNotMet()
    {
        var output = Generate(
            @"
using System.Collections.Generic;
using FxKit.CompilerServices;

namespace Super.Duper.Unions;

[Union]
public record MissingPartial
{
    partial record Oops;
}

[Union]
public sealed record DeclaredAsSealed
{
    partial record Oops;
}

[Union]
public partial record HasPrimaryCtor(string NotAllowed)
{
    partial record Oops;
}

public record TheBaseType;

[Union]
public partial record InheritsFromAnotherType : TheBaseType
{
    partial record Oops;
}
");
        output.Should().BeEmpty();
    }

    [Test]
    public void DoesNotThrowOnBadlyFormattedCode()
    {
        var output = Generate(
            @"
using System.Collections.Generic;
using FxKit.CompilerServices;

namespace Super.Duper.Unions;

[Union]
public record BadFormatting(st
{
    partial record Oops;
}
");
        output.Should().BeEmpty();
    }

    private static string Generate(string source) =>
        CodeGeneratorTestUtil.GetGeneratedOutput(new UnionGenerator(), source);
}
