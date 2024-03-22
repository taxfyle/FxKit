using FluentAssertions;
using FxKit.Identifiers;

namespace FxKit.Tests.UnitTests.Identifiers;

public class DeterministicGuidGeneratorTests
{
    [Test]
    public void Create_ReturnsTheSameIdForTheSameGuidInput()
    {
        var ns = Guid.NewGuid();
        var gen1 = new DeterministicGuidGenerator(ns);
        var gen1Clone = new DeterministicGuidGenerator(ns);
        var gen2 = new DeterministicGuidGenerator(Guid.NewGuid());
        var input = new Guid("3129c5ab-5a6e-42bd-a5f0-addb072c681e");

        gen1.Create(input).Should().Be(gen1.Create(input), "the same instance was used");
        gen1.Create(input).Should().Be(gen1Clone.Create(input), "the same namespace was used");
        gen1.Create(input).Should().NotBe(gen2.Create(input), "a different namespace was used");
    }

    [Test]
    public void Create_ReturnsTheSameIdForTheSameBytesInput()
    {
        var ns = Guid.NewGuid();
        var gen1 = new DeterministicGuidGenerator(ns);
        var gen1Clone = new DeterministicGuidGenerator(ns);

        // Long prefix so we are sure its not just using the first few bytes
        const string Prefix =
            "i wish i was little bit taller i wish i was a baller i wish i had a girl who looked good i would call her i wish i had a rabbit in a hat with a bat and a six four impala";
        const string Input = Prefix + "one";
        const string Other = Prefix + "two";

        gen1.Create(Prefix).Should().Be(gen1.Create(Prefix), "the same input was used");
        gen1.Create(Input).Should().Be(gen1Clone.Create(Input), "the same input was used");

        gen1.Create(Prefix).Should().NotBe(gen1.Create(Input), "the input is different");
        gen1.Create(Input).Should().NotBe(gen1.Create(Other), "the input is different");
    }

    [Test]
    public void Create_IsCompatibleWithSomeRandomOnlineImplementationIFoundSomewhere()
    {
        // From the "V5: Non-Random UUIDs" section on this website: https://www.sohamkamani.com/uuid-versions-explained/#v5-non-random-uuids
        var ns = new Guid("31f4cd21-6444-4aed-9db0-3559438a4dcf");
        var gen = new DeterministicGuidGenerator(ns);
        const string Input = "i cant believe its not butter";
        var expected = new Guid("7047f9ec-eaf4-5f48-9178-92f870955a83");

        gen.Create(Input).Should().Be(expected);
    }
}
