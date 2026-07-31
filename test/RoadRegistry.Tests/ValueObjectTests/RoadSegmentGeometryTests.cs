namespace RoadRegistry.Tests.ValueObjectTests;

using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;
using Xunit;

public class RoadSegmentGeometryTests
{
    private static MultiLineString BuildGeometry()
    {
        return new MultiLineString([
            new LineString([new Coordinate(0, 0), new Coordinate(30, 40), new Coordinate(60, 80)])
        ]).WithSrid(WellknownSrids.Lambert08);
    }

    [Fact]
    public void Value_IsPlain2D_WithoutMeasureOrdinates()
    {
        // Road registry v2 geometries are plain 2D: the value object must not decorate them with an M
        // (measure/chainage) ordinate, which used to leak into the read-model databases.
        var geometry = RoadSegmentGeometry.Create(BuildGeometry());

        geometry.Value.Coordinates.Should().OnlyContain(c => double.IsNaN(c.M), "no M ordinate may be present");
        geometry.Value.Coordinates.Should().OnlyContain(c => double.IsNaN(c.Z), "no Z ordinate may be present");
    }

    [Fact]
    public void Value_KeepsSridOnTheGeometryAndItsLineStrings()
    {
        // The SRID must still reach the child line strings: callers such as GetSingleLineString() rely on it.
        var geometry = RoadSegmentGeometry.Create(BuildGeometry());

        geometry.Value.SRID.Should().Be(WellknownSrids.Lambert08);
        geometry.Value.Geometries.Should().OnlyContain(x => x.SRID == WellknownSrids.Lambert08);
        geometry.Value.GetSingleLineString().SRID.Should().Be(WellknownSrids.Lambert08);
    }

    [Fact]
    public void Value_PreservesCoordinatesAndLength()
    {
        var source = BuildGeometry();

        var value = RoadSegmentGeometry.Create(source).Value;

        value.Length.Should().BeApproximately(source.Length, 0.001);
        value.Coordinates.Length.Should().Be(source.Coordinates.Length);
        value.Coordinates[0].X.Should().Be(0);
        value.Coordinates[^1].X.Should().Be(60);
        value.Coordinates[^1].Y.Should().Be(80);
    }
}
