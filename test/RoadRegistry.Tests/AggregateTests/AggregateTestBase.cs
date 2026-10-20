namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;

public abstract class AggregateTestBase
{
    // What the scopes on a caller's token amount to. Most tests here are not about the scopes at all, so they hand
    // the domain a caller holding both.
    protected static readonly EditableGeometryDrawMethods AllGeometryDrawMethods = new(Ingeschetst: true, Ingemeten: true);
    protected static readonly EditableGeometryDrawMethods OnlyIngeschetst = new(Ingeschetst: true, Ingemeten: false);
    protected static readonly EditableGeometryDrawMethods OnlyIngemeten = new(Ingeschetst: false, Ingemeten: true);

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
