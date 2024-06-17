using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FxKit.CompilerServices.Tests.TestUtils;

public static class CompilationUtil
{
    /// <summary>
    ///     Creates a compilation that references all the assemblies that the test project does.
    /// </summary>
    /// <param name="sourceCode"></param>
    /// <returns></returns>
    public static CSharpCompilation CreateCompilation(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = Basic.Reference.Assemblies.Net80.References.All
            .CastArray<MetadataReference>()
            // Add a reference to the `Annotations` assembly.
            .Add(
                MetadataReference.CreateFromFile(
                    typeof(GenerateTransformerAttribute).Assembly.Location));


        var compilation = CSharpCompilation.Create(
            $"RoslynTests_{Guid.NewGuid():N}",
            new[]
            {
                syntaxTree
            },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation;
    }
}
