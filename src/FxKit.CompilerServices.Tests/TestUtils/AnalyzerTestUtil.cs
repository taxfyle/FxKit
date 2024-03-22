using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FxKit.CompilerServices.Tests.TestUtils;

public static class AnalyzerTestUtil
{
    /// <summary>
    ///     Analyzes the source using the specified analyzer.
    /// </summary>
    /// <param name="analyzer"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static async Task<IReadOnlyList<Diagnostic>> GetDiagnosticsAsync(
        DiagnosticAnalyzer analyzer,
        string source)
    {
        var compilation = CompilationUtil.CreateCompilation(source)
            .WithAnalyzers(ImmutableArray.Create(analyzer));
        return await compilation.GetAnalyzerDiagnosticsAsync();
    }
}
