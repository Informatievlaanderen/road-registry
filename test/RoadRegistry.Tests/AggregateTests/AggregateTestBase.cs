namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using Extensions;
using NetTopologySuite.Geometries;

public abstract class AggregateTestBase
{
    protected RoadNetworkTestDataV2 TestData { get; }
    protected IFixture Fixture { get; }

    protected AggregateTestBase()
    {
        TestData = new();
        Fixture = TestData.Fixture;
    }

    protected RoadSegmentGeometry BuildRoadSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }
    protected RoadSegmentGeometry BuildRoadSegmentGeometry(Point[] points)
    {
        return new MultiLineString([new LineString(points.Select(x => x.Coordinate).ToArray())])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }
}
