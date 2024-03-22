using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FxKit.CompilerServices.Tests.TestUtils;

public static class CodeGeneratorTestUtil
{
    /// <summary>
    ///     Uses the generator to generate source code from the input.
    ///     Returns the generated code, or <c>null</c> if there should be no code generated.
    /// </summary>
    /// <param name="generator"></param>
    /// <param name="sourceCode"></param>
    /// <returns></returns>
    public static string GetGeneratedOutput(ISourceGenerator generator, string sourceCode)
    {
        var compilation = CompilationUtil.CreateCompilation(sourceCode);

        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should()
            .BeEmpty();

        return OutputToString(outputCompilation);
    }

    /// <summary>
    ///     Uses the generator to generate source code from the input.
    ///     Returns the generated code, or <c>null</c> if there should be no code generated.
    /// </summary>
    /// <param name="generator"></param>
    /// <param name="sourceCode"></param>
    /// <returns></returns>
    public static string GetGeneratedOutput(IIncrementalGenerator generator, string sourceCode)
    {
        var compilation = CompilationUtil.CreateCompilation(sourceCode);

        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should()
            .BeEmpty();

        return OutputToString(outputCompilation);
    }

    /// <summary>
    ///     Combines all the output syntax trees to a string.
    /// </summary>
    /// <param name="outputCompilation"></param>
    /// <returns></returns>
    private static string OutputToString(Compilation outputCompilation) => string.Join(
        "\n\n-------------\n\n",
        outputCompilation.SyntaxTrees.Skip(1));
}
