using FluentAssertions;
using FxKit.Testing.FluentAssertions;

namespace FxKit.Tests.UnitTests;

public class ListTraverseTests
{
    [Test]
    public void TraverseOption_ReturnsNoneWhenAnyOfTheInnerItemsMapToNone()
    {
        var listOfMixedNumbers = new[]
        {
            1, 2, 3
        };

        var listOfOnlyEvenNumbers =
            listOfMixedNumbers.Traverse(value => value % 2 == 0 ? Some(value) : None);
        listOfOnlyEvenNumbers.Should().BeNone();
    }

    [Test]
    public void TraverseOption_ReturnsTheListOfNormalValuesWhenAllInnerItemsMapToSome()
    {
        var listOfEvenNumbers = new[]
        {
            2, 4, 6
        };

        var listOfOnlyEvenNumbers =
            listOfEvenNumbers.Traverse(value => value % 2 == 0 ? Some(value) : None);
        var result = listOfOnlyEvenNumbers.Should().BeSome();
        result.Should().BeEquivalentTo(listOfEvenNumbers);
    }
}
