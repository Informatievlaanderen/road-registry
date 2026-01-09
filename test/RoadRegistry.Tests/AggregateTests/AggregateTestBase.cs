namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using Extensions;
using NetTopologySuite.Geometries;

public abstract class AggregateTestBase
{
    protected RoadNetworkTestData TestData { get; }
    protected IFixture Fixture { get; }

    protected AggregateTestBase()
    {
        TestData = new();
        Fixture = TestData.Fixture;
    }

    protected MultiLineString BuildMultiLineString(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates();
    }
}
