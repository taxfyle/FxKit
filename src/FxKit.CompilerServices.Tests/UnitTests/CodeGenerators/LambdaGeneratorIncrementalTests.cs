using FluentAssertions;
using FxKit.CompilerServices.CodeGenerators.Lambdas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class LambdaGeneratorIncrementalTests
{
    [Test]
    public void GeneratorIsIncremental()
    {
        var generator = new LambdaGenerator();
        var originalTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using FxKit.CompilerServices;

            namespace App;

            [Lambda]
            public partial record Foo(string Param1, int Param2);
            """);
        var compilation = CreateCompilation(
        [
            originalTree,
        ]);

        // Run it once.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: default,
                trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);

        // Update compilation with irrelevant change and rerun.
        {
            compilation =
                compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// nothing interesting"));
            driver = driver.RunGenerators(compilation);

            // Assert that we didn't recompute the output.
            var result = driver.GetRunResult().Results.Single();
            var outputs = from step in result.TrackedOutputSteps.Values.SelectMany(x => x)
                from outputStep in step.Outputs
                select outputStep;
            outputs.Should()
                .AllSatisfy(output => output.Reason.Should().Be(IncrementalStepRunReason.Cached));

            // Assert that the driver used the cached steps as expected.
            var methodsByFunctorOutputs = result.TrackedSteps["MethodDescriptors"].Single();
            methodsByFunctorOutputs.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Unchanged);

            var grouped = result.TrackedSteps["Grouped"].Single();
            grouped.Outputs.Single().Reason.Should().Be(IncrementalStepRunReason.Cached);
        }

        // Add another type with a lambda attribute.
        {
            compilation =
                compilation.AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(
                        """
                        using System;
                        using FxKit.CompilerServices;

                        namespace AnotherNamespace;

                        [Lambda] // doesn't matter that the name is the same, it's a different namespace
                        public partial record Foo(string Param1, int Param2);
                        """));
            driver = driver.RunGenerators(compilation);
            var result = driver.GetRunResult().Results.Single();

            // Assert that the groups were re-transformed but only new code generated.
            result.TrackedSteps["MethodDescriptors"].Should().HaveCount(2);
            var originalDescriptor = result.TrackedSteps["MethodDescriptors"][0];
            originalDescriptor.Outputs.Single().Reason.Should().Be(IncrementalStepRunReason.Unchanged);
            var newDescriptor = result.TrackedSteps["MethodDescriptors"][1];
            newDescriptor.Outputs.Single().Reason.Should().Be(IncrementalStepRunReason.New);

            var grouped = result.TrackedSteps["Grouped"].Single();
            var originalGeneration = grouped.Outputs[0];
            originalGeneration.Reason.Should().Be(IncrementalStepRunReason.Unchanged);

            var newGeneration = grouped.Outputs[1];
            newGeneration.Reason.Should().Be(IncrementalStepRunReason.New);
        }

        // Modify the existing type.
        {
            compilation =
                compilation.ReplaceSyntaxTree(
                    originalTree,
                    CSharpSyntaxTree.ParseText(
                        """
                        using System;
                        using System.Collections.Generic;
                        using FxKit.CompilerServices;

                        namespace App;

                        [Lambda]
                        public partial record Foo(string Param1, int Param2)
                        {
                            [Lambda]
                            public static List<Foo> CreateList(Foo input) => [input];
                        }
                        """));
            driver = driver.RunGenerators(compilation);
            var result = driver.GetRunResult().Results.Single();

            // Assert that the groups were re-transformed but only new code generated.
            result.TrackedSteps["MethodDescriptors"].Should().HaveCount(2);
            var retransformedDescriptor = result.TrackedSteps["MethodDescriptors"][0];
            retransformedDescriptor.Outputs.Should().HaveCount(2);
            retransformedDescriptor.Outputs[0].Reason.Should().Be(IncrementalStepRunReason.Unchanged);
            retransformedDescriptor.Outputs[1].Reason.Should().Be(IncrementalStepRunReason.New);

            var newDescriptor = result.TrackedSteps["MethodDescriptors"][1];
            newDescriptor.Outputs.Single().Reason.Should().Be(IncrementalStepRunReason.Unchanged);

            var grouped = result.TrackedSteps["Grouped"].Single();
            var retransformedGeneration = grouped.Outputs[0];
            retransformedGeneration.Reason.Should().Be(IncrementalStepRunReason.Modified);

            var unchanged = grouped.Outputs[1];
            unchanged.Reason.Should().Be(IncrementalStepRunReason.Unchanged);
        }
    }

    private static CSharpCompilation CreateCompilation(params SyntaxTree[] sources)
    {
        return CSharpCompilation.Create(
            assemblyName: $"LambdaGeneratorTest_{Guid.NewGuid():N}",
            syntaxTrees: sources,
            references:
            [
                Basic.Reference.Assemblies.Net80.References.mscorlib,
                Basic.Reference.Assemblies.Net80.References.System,
                Basic.Reference.Assemblies.Net80.References.SystemCollections,
                MetadataReference.CreateFromFile(typeof(LambdaAttribute).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
