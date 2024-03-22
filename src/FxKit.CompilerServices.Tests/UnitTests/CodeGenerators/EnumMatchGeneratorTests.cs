using FluentAssertions;
using FxKit.CompilerServices.CodeGenerators;
using FxKit.CompilerServices.Tests.TestUtils;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class EnumMatchGeneratorTests
{
    [Test]
    public async Task GeneratesMatchExtensionClass()
    {
        var generated = Generate(
            @"
using System;
using FxKit.CompilerServices;

namespace EnumTest.PrettyCool;

[EnumMatch]
public enum MyEnum
{{
    One,
    Two,
    Three
}}
");

        await generated.VerifyGeneratedCode();
    }

    [Test]
    public void DoesNotGenerateForNonEnumTypes()
    {
        var generated = Generate(
            @"
using System;
using FxKit.CompilerServices;

namespace EnumTest.PrettyCool;

[EnumMatch]
public record NotAnEnum;
");

        generated.Should().BeEmpty();
    }

    [Test]
    public async Task SupportsNestedTypes()
    {
        var generated = Generate(
            @"
using System;
using FxKit.CompilerServices;

namespace EnumTest.PrettyCool;

public class SuperNested<T>
{{
    [EnumMatch]
    public enum MyEnum
    {{
        One,
        Two,
        Three
    }}
}}
");

        await generated.VerifyGeneratedCode();
    }

    private static string Generate(string source) =>
        CodeGeneratorTestUtil.GetGeneratedOutput(new EnumMatchGenerator(), source);
}
