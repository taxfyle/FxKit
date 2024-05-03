using FluentAssertions;
using FxKit.Collections;

namespace FxKit.Tests.UnitTests.Collections;

public class InsertionOrderedDictionaryTests
{
    [Test]
    public void MaintainsInsertionOrderWhenEnumerated()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        IReadOnlyDictionary<int, string> source = mutableSource;

        source.Keys.Should().Equal(1, 2);
        source.Values.Should().Equal("One", "Two");
    }

    [Test]
    public void AccessesValuesByKey()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        IReadOnlyDictionary<int, string> source = mutableSource;

        source[1].Should().Be("One");
        source[2].Should().Be("Two");
    }

    [Test]
    public void ThrowsExceptionForNonExistentKey()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        IReadOnlyDictionary<int, string> source = mutableSource;

        FluentActions.Invoking(() => source[3]).Should().Throw<KeyNotFoundException>();
    }

    [Test]
    public void AddsNewKeyValuePair()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        mutableSource[69] = "Nice";

        mutableSource[69].Should().Be("Nice");
        mutableSource.Count.Should().Be(3);
    }

    [Test]
    public void RemovesKeyValuePair()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        mutableSource.Remove(2).Should().BeTrue();
        mutableSource.Count.Should().Be(1);
    }

    [Test]
    public void DoesNotRemoveNonExistentKeyValuePair()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        mutableSource.Remove(420).Should().BeFalse();
        mutableSource.Count.Should().Be(2);
    }

    [Test]
    public void ClearsAllKeyValuePairs()
    {
        var mutableSource = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        mutableSource.Clear();

        mutableSource.Count.Should().Be(0);
    }

    [Test]
    public void Enumeration()
    {
        IReadOnlyDictionary<int, string> source = new InsertionOrderedDictionary<int, string>
        {
            {
                1, "One"
            },
            {
                2, "Two"
            }
        };
        source.Select(x => x.Value).Should().Equal("One", "Two");
    }
}
