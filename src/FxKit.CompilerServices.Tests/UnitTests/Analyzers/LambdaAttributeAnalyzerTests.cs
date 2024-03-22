using FluentAssertions;
using FxKit.CompilerServices.Analyzers;
using FxKit.CompilerServices.Tests.TestUtils;
using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices.Tests.UnitTests.Analyzers;

public class LambdaAttributeAnalyzerTests
{
    [Test]
    public async Task DoesNotReportWhenUsedCorrectly()
    {
        const string Source = @"
using FxKit.CompilerServices;

namespace Super.Duper.Lambdas;

public partial class MyClass
{
    [Lambda]
    public static int DefinitelyStatic(int arg) => arg;
}

[Lambda]
public partial record MyRecord(int MyParameter);

[Lambda]
public partial class MyClassWithPrimaryCtor(int MyParameter);
";
        var diagnostics = await Analyze(Source);
        diagnostics.Should().BeEmpty();
    }

    [Test]
    public async Task ReportsWhenMissingStaticOnMethod()
    {
        const string Source = @"
using FxKit.CompilerServices;

namespace Super.Duper.Lambdas;

public partial class MyClass
{
    [Lambda]
    public int NotStatic(int arg) => arg;
}
";
        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    private static Task<IReadOnlyList<Diagnostic>> Analyze(string source) =>
        AnalyzerTestUtil.GetDiagnosticsAsync(new LambdaAttributeAnalyzer(), source);
}
