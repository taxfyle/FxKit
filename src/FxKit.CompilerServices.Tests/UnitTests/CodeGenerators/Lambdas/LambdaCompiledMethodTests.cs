using FluentAssertions;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators.Lambdas;

public partial class LambdaCompiledMethodTests
{
    [Test]
    public void CallsTheMethod()
    {
        var result = MyClass<string>.Createλ("one", arg2: 1)[0];
        result.Value.Should().Be("one");
        result.Number.Should().Be(1);
    }

    private partial class MyClass<T>
    {
        public T   Value  { get; }
        public int Number { get; }

        private MyClass(T value, int number)
        {
            Value = value;
            Number = number;
        }

        [Lambda]
        public static List<MyClass<T>> Create(T arg, int arg2)
        {
            return
            [
                ..new[]
                {
                    new MyClass<T>(arg, arg2)
                }
            ];
        }
    }
}
