using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices.Tests.TestUtils;

public static class VerifyHelpers
{
    /// <summary>
    ///     Verifies the source is what was previously recorded.
    /// </summary>
    /// <param name="actualSource"></param>
    /// <param name="snapName">Extra name to add to the snapshot file.</param>
    /// <param name="sourceFile"></param>
    /// <param name="methodName"></param>
    public static async Task VerifyGeneratedCode(
        this string actualSource,
        string? snapName = null,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string methodName = "")
    {
        var settings = new VerifySettings();
        settings.UseMethodName(methodName);
        if (snapName is not null)
        {
            settings.UseTextForParameters(snapName);
        }

        // ReSharper disable once ExplicitCallerInfoArgument
        await Verify(actualSource, settings, sourceFile);
    }

    /// <summary>
    ///     Verifies the diagnostics are the same as previously recorded.
    /// </summary>
    /// <param name="diagnostics"></param>
    /// <param name="snapName">Extra name to add to the snapshot file.</param>
    /// <param name="sourceFile"></param>
    /// <param name="methodName"></param>
    public static async Task VerifyDiagnostics(
        this IEnumerable<Diagnostic> diagnostics,
        string? snapName = null,
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string methodName = "")
    {
        var settings = new VerifySettings();
        settings.UseMethodName(methodName);
        if (snapName is not null)
        {
            settings.UseTextForParameters(snapName);
        }

        // ReSharper disable once ExplicitCallerInfoArgument
        await Verify(diagnostics.Select(d => d.ToString()).OrderBy(b => b), settings, sourceFile);
    }
}
