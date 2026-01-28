namespace RoadRegistry.Projections.Tests.Projections.ExtractRoadSegment;

using AutoFixture;
using Extensions;
using Extracts.Projections;
using Extracts.ZipArchiveWriters;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Tests.AggregateTests;
using RoadSegment.ValueObjects;

public class RoadSegmentFlattenerTests
{
    [Fact]
    public void WithOnePart_ThenReturnOne()
    {
        var geometry = BuildRoadSegmentGeometry(new Point(0, 0), new Point(100, 0));
        var accessRestriction = new ExtractRoadSegmentDynamicAttribute<string>("1", geometry);

        var segment = new RoadSegmentExtractItem
        {
            RoadSegmentId = new(1),
            Geometry = geometry,
            StartNodeId = new(1),
            EndNodeId = new(2),
            GeometryDrawMethod = null,
            AccessRestriction = accessRestriction,
            Category = new(),
            Morphology = new(),
            Status = new(),
            StreetNameId = new(),
            MaintenanceAuthorityId = new(),
            SurfaceType = new(),
            CarAccess = new(),
            BikeAccess = new(),
            PedestrianAccess = new(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = null,
            LastModified = null,
            IsV2 = true
        };

        var flatSegments = segment.Flatten();
        flatSegments.Should().HaveCount(1);

        flatSegments[0].Geometry.Should().BeEquivalentTo(geometry);
        flatSegments[0].AccessRestriction.Should().Be("1");
    }

    [Fact]
    public void WithMultiplePartsOverDifferentAttributes_ThenReturnExpected()
    {
        var geometry = BuildRoadSegmentGeometry(new Point(0, 0), new Point(100, 0));

        var accessRestriction = new ExtractRoadSegmentDynamicAttribute<string>([
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(20), RoadSegmentAttributeSide.Both, "1"),
            (RoadSegmentPosition.FromDouble(20), RoadSegmentPosition.FromDouble(100), RoadSegmentAttributeSide.Both, "2"),
        ]);

        var category = new ExtractRoadSegmentDynamicAttribute<string>([
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(50), RoadSegmentAttributeSide.Both, "a"),
            (RoadSegmentPosition.FromDouble(50), RoadSegmentPosition.FromDouble(100), RoadSegmentAttributeSide.Both, "b"),
        ]);

        var segment = new RoadSegmentExtractItem
        {
            RoadSegmentId = new(1),
            Geometry = geometry,
            StartNodeId = new(1),
            EndNodeId = new(2),
            GeometryDrawMethod = null,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = new(),
            Status = new(),
            StreetNameId = new(),
            MaintenanceAuthorityId = new(),
            SurfaceType = new(),
            CarAccess = new(),
            BikeAccess = new(),
            PedestrianAccess = new(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = null,
            LastModified = null,
            IsV2 = true
        };

        var flatSegments = segment.Flatten();
        flatSegments.Should().HaveCount(3);

        var segment1 = flatSegments[0];
        segment1.Geometry.Value.Coordinates.First().Should().Be(new Coordinate(0, 0));
        segment1.Geometry.Value.Coordinates.Last().Should().Be(new Coordinate(20, 0));
        segment1.AccessRestriction.Should().Be("1");
        segment1.Category.Should().Be("a");

        var segment2 = flatSegments[1];
        segment2.Geometry.Value.Coordinates.First().Should().Be(new Coordinate(20, 0));
        segment2.Geometry.Value.Coordinates.Last().Should().Be(new Coordinate(50, 0));
        segment2.AccessRestriction.Should().Be("2");
        segment2.Category.Should().Be("a");

        var segment3 = flatSegments[2];
        segment3.Geometry.Value.Coordinates.First().Should().Be(new Coordinate(50, 0));
        segment3.Geometry.Value.Coordinates.Last().Should().Be(new Coordinate(100, 0));
        segment3.AccessRestriction.Should().Be("2");
        segment3.Category.Should().Be("b");
    }

    [Fact]
    public void WithLeftAndRight_ThenReturnExpected()
    {
        var geometry = BuildRoadSegmentGeometry(new Point(0, 0), new Point(100, 0));

        var accessRestriction = new ExtractRoadSegmentDynamicAttribute<string>([
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(20), RoadSegmentAttributeSide.Both, "1"),
            (RoadSegmentPosition.FromDouble(20), RoadSegmentPosition.FromDouble(100), RoadSegmentAttributeSide.Both, "2"),
        ]);

        var streetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>([
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(50), RoadSegmentAttributeSide.Left, new StreetNameLocalId(20)),
            (RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(50), RoadSegmentAttributeSide.Right, new StreetNameLocalId(25)),
            (RoadSegmentPosition.FromDouble(50), RoadSegmentPosition.FromDouble(100), RoadSegmentAttributeSide.Both, new StreetNameLocalId(30)),
        ]);

        var segment = new RoadSegmentExtractItem
        {
            RoadSegmentId = new(1),
            Geometry = geometry,
            StartNodeId = new(1),
            EndNodeId = new(2),
            GeometryDrawMethod = null,
            AccessRestriction = accessRestriction,
            Category = new(),
            Morphology = new(),
            Status = new(),
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = new(),
            SurfaceType = new(),
            CarAccess = new(),
            BikeAccess = new(),
            PedestrianAccess = new(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = null,
            LastModified = null,
            IsV2 = true
        };

        var flatSegments = segment.Flatten();
        flatSegments.Should().HaveCount(3);

        var segment1 = flatSegments[0];
        segment1.Geometry.Value.Coordinates.First().Should().Be(new Coordinate(0, 0));
        segment1.Geometry.Value.Coordinates.Last().Should().Be(new Coordinate(20, 0));
        segment1.AccessRestriction.Should().Be("1");
        segment1.LeftStreetNameId.Should().Be(new StreetNameLocalId(20));
        segment1.RightStreetNameId.Should().Be(new StreetNameLocalId(25));

        var segment2 = flatSegments[1];
        segment2.Geometry.Value.Coordinates.First().Should().Be(new Coordinate(20, 0));
        segment2.Geometry.Value.Coordinates.Last().Should().Be(new Coordinate(50, 0));
        segment2.AccessRestriction.Should().Be("2");
        segment2.LeftStreetNameId.Should().Be(new StreetNameLocalId(20));
        segment2.RightStreetNameId.Should().Be(new StreetNameLocalId(25));

        var segment3 = flatSegments[2];
        segment3.Geometry.Value.Coordinates.First().Should().Be(new Coordinate(50, 0));
        segment3.Geometry.Value.Coordinates.Last().Should().Be(new Coordinate(100, 0));
        segment3.AccessRestriction.Should().Be("2");
        segment3.LeftStreetNameId.Should().Be(new StreetNameLocalId(30));
        segment3.RightStreetNameId.Should().Be(new StreetNameLocalId(30));
    }

    private static RoadSegmentGeometry BuildRoadSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .ToRoadSegmentGeometry();
    }
}
