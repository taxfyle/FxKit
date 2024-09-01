using FluentAssertions;
using FxKit.CompilerServices.Analyzers;
using FxKit.CompilerServices.Tests.TestUtils;
using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices.Tests.UnitTests.Analyzers;

public class UnionAnalyzerTests
{
    [Test]
    public async Task DoesNotReportForCorrectUsage()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            [Union]
            public partial record ForgotToAddPartial
            {
                partial record Woah(string Parameter);
                partial record Right;
            }
            """;

        var diagnostics = await Analyze(Source);
        diagnostics.Should().BeEmpty();
    }

    [Test]
    public async Task ReportsForInvalidConstituent()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            [Union]
            public partial record ForgotToAddPartial
            {
                public record Woah(string Parameter); // missing partial keyword
                partial record Right;
            }
            """;

        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    [Test]
    public async Task ReportsForMissingConstituents()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            [Union]
            public partial record MissingConstituents
            {
            }
            """;

        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    [Test]
    public async Task ReportsForNonClosedUsage()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            [Union]
            public partial record NonClosed
            {
                partial record Correct;
            }

            public record NonClosedUsage : NonClosed;
            """;

        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    [Test]
    public async Task ReportsWhenDeclaringPrimaryConstructor()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            [Union]
            public partial record HasCtor(bool NotAllowed)
            {
                partial record Incorrect;
            }
            """;

        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    [Test]
    public async Task ReportsWhenDeclaringBaseType()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            public record TheBaseType;

            [Union]
            public partial record HaseBaseType : TheBaseType
            {
                partial record Incorrect;
            }

            [Union]
            public partial record HaseBaseTypeButItDoesntEvenCompile : DontKnowWhatThisIs
            {
                partial record Incorrect;
            }

            """;

        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    [Test]
    public async Task ReportsWhenInvalidModifiersAreUsed()
    {
        const string Source =
            """
            using FxKit.CompilerServices;

            namespace Super.Duper.Unions;

            [Union]
            public partial sealed record IsSealed
            {
              partial record Incorrect;
            }

            [Union]
            public partial abstract record IsAbstract
            {
              partial record Incorrect;
            }
            """;

        var diagnostics = await Analyze(Source);
        await diagnostics.VerifyDiagnostics();
    }

    private static Task<IReadOnlyList<Diagnostic>> Analyze(string source) =>
        AnalyzerTestUtil.GetDiagnosticsAsync(new UnionAnalyzer(), source);
}
