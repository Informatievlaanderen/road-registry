namespace RoadRegistry.Editor.ProjectionHost.Tests.Schema.RoadSegments;

using Editor.Schema.RoadSegments;
using FluentAssertions;

public class RoadSegmentRecordEqualityComparerByIdTests
{
    public RoadSegmentRecordEqualityComparerByIdTests()
    {
        _sut = new RoadSegmentRecordEqualityComparerById();
    }

    private readonly RoadSegmentRecordEqualityComparerById _sut;

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Equals_returns_expected_result(RoadSegmentRecord x, RoadSegmentRecord y, bool expected)
    {
        var actual = _sut.Equals(x, y);
        actual.Should().Be(expected);
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        yield return new object[]
        {
            new RoadSegmentRecord { Id = 1 },
            new RoadSegmentRecord { Id = 1 },
            true
        };
        yield return new object[]
        {
            new RoadSegmentRecord { Id = 1 },
            new RoadSegmentRecord { Id = 2 },
            false
        };
        yield return new object[]
        {
            new RoadSegmentRecord { Id = 2 },
            new RoadSegmentRecord { Id = 1 },
            false
        };
        yield return new object[]
        {
            null,
            new RoadSegmentRecord { Id = 1 },
            false
        };
        yield return new object[]
        {
            new RoadSegmentRecord { Id = 1 },
            null,
            false
        };
        yield return new object[] // don't care about properties other than Id
        {
            new RoadSegmentRecord { Id = 1, ShapeRecordContentLength = 100 },
            new RoadSegmentRecord { Id = 1, ShapeRecordContentLength = 50 },
            true
        };

        var roadSegmentRecord = new RoadSegmentRecord { Id = 1 };
        yield return new object[]
        {
            roadSegmentRecord,
            roadSegmentRecord,
            true
        };
    }
}
