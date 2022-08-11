namespace RoadRegistry.Editor.Schema.RoadNodes;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class RoadNodeRecordEqualityComparerByIdTests
{
    private readonly RoadNodeRecordEqualityComparerById _sut;

    public RoadNodeRecordEqualityComparerByIdTests()
    {
        _sut = new RoadNodeRecordEqualityComparerById();
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Equals_returns_expected_result(RoadNodeRecord x, RoadNodeRecord y, bool expected)
    {
        var actual = _sut.Equals(x, y);
        actual.Should().Be(expected);
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        yield return new object[]
        {
            new RoadNodeRecord { Id = 1 },
            new RoadNodeRecord { Id = 1 },
            true
        };
        yield return new object[]
        {
            new RoadNodeRecord { Id = 1 },
            new RoadNodeRecord { Id = 2 },
            false
        };
        yield return new object[]
        {
            new RoadNodeRecord { Id = 2 },
            new RoadNodeRecord { Id = 1 },
            false
        };
        yield return new object[]
        {
            null,
            new RoadNodeRecord { Id = 1 },
            false
        };
        yield return new object[]
        {
            new RoadNodeRecord { Id = 1 },
            null,
            false
        };
        yield return new object[] // don't care about properties other than Id
        {
            new RoadNodeRecord { Id = 1, ShapeRecordContentLength = 100 },
            new RoadNodeRecord { Id = 1, ShapeRecordContentLength = 50 },
            true
        };

        var roadNodeRecord = new RoadNodeRecord { Id = 1 };
        yield return new object[]
        {
            roadNodeRecord,
            roadNodeRecord,
            true
        };
    }
}
