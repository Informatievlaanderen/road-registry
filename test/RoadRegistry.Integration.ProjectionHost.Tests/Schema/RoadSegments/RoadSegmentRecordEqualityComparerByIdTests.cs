namespace RoadRegistry.Integration.ProjectionHost.Tests.Schema.RoadSegments;

using Integration.Schema.RoadSegments;
using FluentAssertions;

public class RoadSegmentLatestItemEqualityComparerByIdTests
{
    private readonly RoadSegmentLatestItemEqualityComparerById _sut;

    public RoadSegmentLatestItemEqualityComparerByIdTests()
    {
        _sut = new RoadSegmentLatestItemEqualityComparerById();
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Equals_returns_expected_result(RoadSegmentLatestItem x, RoadSegmentLatestItem y, bool expected)
    {
        var actual = _sut.Equals(x, y);
        actual.Should().Be(expected);
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        yield return new object[]
        {
            new RoadSegmentLatestItem { Id = 1 },
            new RoadSegmentLatestItem { Id = 1 },
            true
        };
        yield return new object[]
        {
            new RoadSegmentLatestItem { Id = 1 },
            new RoadSegmentLatestItem { Id = 2 },
            false
        };
        yield return new object[]
        {
            new RoadSegmentLatestItem { Id = 2 },
            new RoadSegmentLatestItem { Id = 1 },
            false
        };
        yield return new object[]
        {
            null,
            new RoadSegmentLatestItem { Id = 1 },
            false
        };
        yield return new object[]
        {
            new RoadSegmentLatestItem { Id = 1 },
            null,
            false
        };
        yield return new object[] // don't care about properties other than Id
        {
            new RoadSegmentLatestItem { Id = 1 },
            new RoadSegmentLatestItem { Id = 1 },
            true
        };

        var roadSegmentRecord = new RoadSegmentLatestItem { Id = 1 };
        yield return new object[]
        {
            roadSegmentRecord,
            roadSegmentRecord,
            true
        };
    }
}
