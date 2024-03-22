using FluentAssertions;
using FxKit.CompilerServices.Analyzers;
using FxKit.CompilerServices.Tests.TestUtils;
using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices.Tests.UnitTests.Analyzers;

public class MustBePartialAnalyzerTests
{
    [Test]
    public async Task DoesNotReportWhenPartial()
    {
        const string Source = @"
using FxKit.CompilerServices;

namespace Super.Duper.Unions;

[Union]
public partial record ForgotToAddPartial
{
    partial record Woah;
}
";
        var diagnostics = await Analyze(Source);
        diagnostics.Should().BeEmpty();
    }

    [Test]
    public async Task ReportsDiagnostics()
    {
        const string Source = @"
using FxKit.CompilerServices;

namespace Super.Duper.Unions;

[Union]
public record ForgotToAddPartial
{
    partial record Woah;
}
";
        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    private static Task<IReadOnlyList<Diagnostic>> Analyze(string source) =>
        AnalyzerTestUtil.GetDiagnosticsAsync(new MustBePartialAnalyzer(), source);
}
