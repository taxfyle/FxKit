using FluentAssertions;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public partial record Parent
{
    [Lambda] // this will generate a static func λ that news up the record. 
    public partial record RecordWithPrimaryCtor(int SoCool) : Parent;

    [Lambda]
    public readonly partial struct StructWithPrimaryCtor(int soCool)
    {
        public int SoCool { get; } = soCool;
    }

    public abstract class BaseClass(int soCool)
    {
        public int SoCool { get; } = soCool;
    }

    [Lambda]
    public partial class ClassWithPrimaryCtor(int soCool) : BaseClass(soCool);
}

public class LambdaCompiledConstructorTests
{
    [Test]
    public void RecordHasLambdaConstructor() =>
        Enumerable.Range(start: 0, count: 2)
            .Select(Parent.RecordWithPrimaryCtor.λ)
            .Should()
            .Equal(
                new Parent.RecordWithPrimaryCtor(0),
                new Parent.RecordWithPrimaryCtor(1));

    [Test]
    public void StructHasLambdaConstructor() =>
        Enumerable.Range(start: 0, count: 2)
            .Select(Parent.StructWithPrimaryCtor.λ)
            .Should()
            .Equal(
                new Parent.StructWithPrimaryCtor(0),
                new Parent.StructWithPrimaryCtor(1));

    [Test]
    public void ClassHasLambdaConstructor() =>
        Enumerable.Range(start: 0, count: 2)
            .Select(Parent.ClassWithPrimaryCtor.λ)
            .Select<Parent.ClassWithPrimaryCtor, int>(c => c.SoCool)
            .Should()
            .Equal(0, 1);
}
