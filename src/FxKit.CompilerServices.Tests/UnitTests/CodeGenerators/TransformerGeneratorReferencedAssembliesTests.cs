using FluentAssertions;
using FxKit.CompilerServices.CodeGenerators.Transformers;
using FxKit.CompilerServices.Tests.TestUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static FxKit.CompilerServices.Tests.TestUtils.CodeGeneratorTestUtil;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class TransformerGeneratorReferencedAssembliesTests
{
    [Test]
    public async Task IncludesFunctorsAndBehaviorFromReferencedAssemblies()
    {
        var references = CreateCoreReferences();
        var compilation = CreateEndUserCompilation(references);

        // Compile and verify output.
        var generator = new TransformerGenerator();
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        diagnostics.Should().BeEmpty();

        var output = OutputToString(outputCompilation);
        await output.VerifyGeneratedCode();

        // The output compilation will also be decorated with the `ContainsFunctor`
        // attribute since it defines functors.
        Helpers.ContainsFunctors(outputCompilation.ToMetadataReference()).Should().BeTrue();
    }

    [Test]
    public void IncrementalCaching()
    {
        var references = CreateCoreReferences();
        var compilation = CreateEndUserCompilation(references);
        var generator = new TransformerGenerator();

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
            var outputs = (
                from step in result.TrackedOutputSteps.Values.SelectMany(x => x)
                from outputStep in step.Outputs
                select outputStep);
            outputs.Should()
                .AllSatisfy(output => output.Reason.Should().Be(IncrementalStepRunReason.Cached));

            // Assert that the driver used the cached steps as expected.
            var methodsByFunctorOutputs = result.TrackedSteps["MethodsByFunctor"].Single();
            methodsByFunctorOutputs.Outputs.Single().Reason.Should().Be(IncrementalStepRunReason.Cached);

            // Intrinsics don't change between runs.
            var functorCandidatesFromIntrinsicFunctors =
                result.TrackedSteps["FunctorCandidatesFromIntrinsicFunctors"].Single();
            functorCandidatesFromIntrinsicFunctors.Outputs.Should()
                .AllSatisfy(o => o.Reason.Should().Be(IncrementalStepRunReason.Unchanged));

            // Cached since we didn't add more functors at all.
            var referencedFunctors =
                result.TrackedSteps["ReferencedFunctors"].Single();
            referencedFunctors.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Cached);

            // Cached since we didn't add more functors in syntax.
            var functorCandidatesFromSyntax =
                result.TrackedSteps["FunctorCandidatesFromSyntax"].Single();
            functorCandidatesFromSyntax.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Cached);

            // Cached since we didn't add more functors in references.
            var functorCandidatesFromReferences =
                result.TrackedSteps["FunctorCandidatesFromReferences"].Single();
            functorCandidatesFromReferences.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Cached);

            // Cached since we didn't add more `Map` implementations in syntax.
            var functorImplementationLocatorsFromSyntax =
                result.TrackedSteps["FunctorImplementationLocatorsFromSyntax"].Single();
            functorImplementationLocatorsFromSyntax.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Cached);

            // Cached since we didn't add any more `Map` at all.
            var functorImplementationLocators =
                result.TrackedSteps["FunctorImplementationLocators"].Single();
            functorImplementationLocators.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Cached);
        }

        // Add another functor via syntax.
        {
            compilation =
                compilation.AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(
                        """
                        namespace NewFunctors
                        {
                            using System;
                            using FxKit.CompilerServices;

                            [Functor]
                            public struct NewFunctor<T>;

                            public static class NewFunctor
                            {
                                [GenerateTransformer]
                                public static NewFunctor<U> Map<T>(this NewFunctor<T> source, Func<T,U> selector)
                                {
                                    throw new NotImplementedException();
                                }
                            }
                        }
                        """));
            driver = driver.RunGenerators(compilation);

            // Assert that the output was recomputed.
            var result = driver.GetRunResult().Results.Single();
            var outputs = (
                from step in result.TrackedOutputSteps.Values.SelectMany(x => x)
                from outputStep in step.Outputs
                select outputStep).ToList();
            var transformers = outputs[0];
            transformers.Reason.Should().Be(IncrementalStepRunReason.Modified);
            var assemblyAttribute = outputs[1];
            assemblyAttribute.Reason.Should().Be(IncrementalStepRunReason.Cached);

            // Cached since we didn't add more functors in references.
            var functorCandidatesFromReferences =
                result.TrackedSteps["FunctorCandidatesFromReferences"].Single();
            functorCandidatesFromReferences.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Cached);

            // Modified since we added functors via syntax.
            var functorCandidatesFromSyntax =
                result.TrackedSteps["FunctorCandidatesFromSyntax"].Single();
            functorCandidatesFromSyntax.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Modified);

            var functorImplementationLocators =
                result.TrackedSteps["FunctorImplementationLocators"].Single();
            functorImplementationLocators.Outputs.Single()
                .Reason.Should()
                .Be(IncrementalStepRunReason.Modified);
        }
    }

    private static CSharpCompilation CreateEndUserCompilation(IReadOnlyList<MetadataReference> references) =>
        CSharpCompilation.Create(
            assemblyName: $"TransformerGeneratorTest_{Guid.NewGuid():N}",
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(
                    """
                    namespace App
                    {
                        using System;
                        using System.Collections.Generic;
                        using FxKit.CompilerServices;

                        [Functor]
                        public record Filtered<T>(IReadOnlyList<T> Items);

                        public static class FilteredExtensions
                        {
                            [GenerateTransformer]
                            public static async Filtered<U> Map<T, U>(
                                this Filtered<T> source,
                                Func<T, U> selector) => new Filtered<U>(source.Items.Select(selector).ToList())
                        }

                        namespace Inner
                        {
                            [Functor]
                            public record Paged<T>(IReadOnlyList<T> Items);

                            public static class PagedExtensions
                            {
                                [GenerateTransformer]
                                public static async Paged<U> Map<T, U>(
                                    this Paged<T> source,
                                    Func<T, U> selector) => new Paged<U>(source.Items.Select(selector).ToList())
                            }
                        }
                    }
                    """)
            },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static IReadOnlyList<MetadataReference> CreateCoreReferences()
    {
        var references = Basic.Reference.Assemblies.Net80.References.All
            .CastArray<MetadataReference>()
            // Add a reference to the `Annotations` assembly.
            .Add(
                MetadataReference.CreateFromFile(
                    typeof(GenerateTransformerAttribute).Assembly.Location));

        // Create a "core" library where functor behavior is defined.
        var coreAssemblyNamePart1 = $"CoreLibrary_{Guid.NewGuid():N}";
        var coreCompilationPart1 = CSharpCompilation.Create(
            assemblyName: coreAssemblyNamePart1,
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(
                    """
                    [assembly: FxKit.CompilerServices.ContainsFunctors]

                    namespace Core
                    {
                        using System;
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        public static partial class TaskExtensions
                        {
                            public static async Task<U> Map<T, U>(
                                this Task<T> source,
                                Func<T, U> selector) => selector(await source);
                        }

                        namespace Collections
                        {
                            public static partial class EnumerableAndListExtensions
                            {
                                public static IEnumerable<U> Map<T, U>(
                                    this IEnumerable<T> source,
                                    Func<T, U> selector) => source.Select(selector);

                                public static IReadOnlyList<U> Map<T, U>(
                                    this IReadOnlyList<T> source,
                                    Func<T, U> selector) => source.Select(selector).ToList();
                            }
                        }
                    }
                    """)
            },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create another library where another functor is defined, but this one will become
        // a Portable Executable (PE) reference rather than a compilation one.
        var coreAssemblyNamePart2 = $"CoreLibraryAsm_{Guid.NewGuid():N}";
        var coreCompilationPart2 = CSharpCompilation.Create(
            assemblyName: coreAssemblyNamePart2,
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(
                    """
                    [assembly: FxKit.CompilerServices.ContainsFunctors]

                    namespace Monads
                    {
                        using System;
                        using FxKit.CompilerServices;

                        [Functor]
                        public struct Option<T> where T : notnull {}

                        public static partial class Option
                        {
                            [GenerateTransformer]
                            public static Option<U> Map<T, U>(this Option<T> source, Func<T, U> selector)
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }

                    namespace Other
                    {
                        public static class Irrelevant
                        {
                            // Should not be considered for anything.
                            public static bool Map(string v) => v == "true";
                        }
                    }
                    """)
            },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create a reference to the first core library.
        var coreLibReference1 = coreCompilationPart1.ToMetadataReference();

        // Emit the 2nd library to a PE stream
        using var dest = new MemoryStream();
        var emitResult = coreCompilationPart2.Emit(dest);
        emitResult.Diagnostics.Should().BeEmpty();
        emitResult.Success.Should().BeTrue();
        dest.Seek(offset: 0, SeekOrigin.Begin);

        var peRef = MetadataReference.CreateFromStream(dest);
        return references.Add(coreLibReference1).Add(peRef);
    }
}
