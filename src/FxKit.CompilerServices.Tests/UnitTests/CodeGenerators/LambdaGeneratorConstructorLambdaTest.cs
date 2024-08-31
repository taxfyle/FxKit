using FluentAssertions;
using FxKit.CompilerServices.CodeGenerators.Lambdas;
using FxKit.CompilerServices.Tests.TestUtils;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class LambdaGeneratorConstructorLambdaTest
{
    [Test]
    public async Task GeneratesTheLambdaMethod()
    {
        const string InputSource =
            """
            using System.Collections.Generic;
            using FxKit.CompilerServices;

            namespace Woah.SoCool;

            [Lambda]
            public partial record MyRecord(
                string Param1,
                int Param2,
                List<string?> Param3
            );
            """;

        var generated = Generate(InputSource);
        generated.Should().NotBeNull();
        await generated.VerifyGeneratedCode();
    }

    [Test]
    public async Task SupportsNestedTypes()
    {
        const string InputSource =
            """
            using System.Collections.Generic;
            using FxKit.CompilerServices;

            namespace Woah.SoCool;

            public static partial class Nested
            {
                [Lambda]
                public partial record MyRecord(
                    string Param1,
                    int Param2,
                    List<string?> Param3
                );
            }
            """;

        var generated = Generate(InputSource);
        await generated.VerifyGeneratedCode();
    }

    [Test]
    public async Task SupportsGenericTypes()
    {
        const string InputSource =
            """
            using System.Collections.Generic;
            using FxKit.CompilerServices;

            namespace Woah.SoCool;

            [Lambda]
            public partial record MyRecord<T>(
                string Param1,
                int Param2,
                List<string?> Param3
            ) where T : notnull;
            """;

        var generated = Generate(InputSource);
        await generated.VerifyGeneratedCode();
    }

    [Test]
    public async Task SupportsClasses()
    {
        const string InputSource =
            """
            using System.Collections.Generic;
            using FxKit.CompilerServices;

            namespace Woah.SoCool.HellaNamespace;

            [Lambda]
            public partial class MyClass<T> where T : notnull
            {
                public MyClass(string param1, List<string> param2) {}
            }
            """;

        var generated = Generate(InputSource);
        await generated.VerifyGeneratedCode();
    }

    [Test]
    public async Task SupportsClassesWithPrimaryConstructor()
    {
        const string InputSource =
            """
            using System.Collections.Generic;
            using FxKit.CompilerServices;

            namespace Woah.SoCool.HellaNamespace;

            [Lambda]
            public partial class MyClass<T>(T param1, List<T> param2) where T : notnull;
            """;

        var generated = Generate(InputSource);
        await generated.VerifyGeneratedCode();
    }

    [Test]
    public async Task SupportsStructsWithPrimaryConstructor()
    {
        const string InputSource =
            """
            using System.Collections.Generic;
            using FxKit.CompilerServices;

            namespace Woah.SoCool.HellaNamespace;

            [Lambda]
            public readonly partial struct MyStruct<T>(T param1) where T : notnull
            {
                public T Value = param1;
            }
            """;

        var generated = Generate(InputSource);
        await generated.VerifyGeneratedCode();
    }

    private static string Generate(string source) =>
        CodeGeneratorTestUtil.GetGeneratedOutput(new LambdaGenerator(), source);
}
