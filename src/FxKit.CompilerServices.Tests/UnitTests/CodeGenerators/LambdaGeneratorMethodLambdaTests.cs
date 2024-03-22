using FxKit.CompilerServices.CodeGenerators;
using FxKit.CompilerServices.Tests.TestUtils;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class LambdaGeneratorMethodLambdaTests
{
    [Test]
    public async Task GeneratesTheLambdaMethod()
    {
        const string InputSource = @"
using System.Collections.Generic;
using FxKit.CompilerServices;

namespace Woah.SoCool.HellaNamespace;

public partial class Container 
{
    public partial class MyClass<T> where T : notnull
    {
        [Lambda]
        public static MyClass<T> Create<R>(List<R> arg1, int arg2, string arg3) 
        {
            throw new Exception();
        }
    }
}
";

        var generated = Generate(InputSource);
        await generated.VerifyGeneratedCode();
    }

    private static string Generate(string source) =>
        CodeGeneratorTestUtil.GetGeneratedOutput(new LambdaGenerator(), source);
}
