namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.ValueObjects;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema.Records;
using Xunit;

// Guards the V1-data handling in DerivedWegsegmentFlattener: stored dynamic-attribute ToPositions do not always line
// up with the actual segment geometry length (the last one can be shorter or longer, e.g. after a geometry change
// that only rewrote some attributes). The flattener must snap each attribute's trailing coverage to the geometry
// length so the flattened rows cover exactly [0, length] and no tail sub-range loses its attribute value.
public class DerivedWegsegmentFlattenerTests
{
    private static int Forward => RoadSegmentTrafficDirection.Forward.Translation.Identifier;

    private static LineString Line(double length) =>
        new([new Coordinate(0, 0), new Coordinate(length, 0)]);

    private static List<DerivedRoadSegmentRecord> Flatten(
        Geometry geometry,
        IReadOnlyList<RoadSegmentMorphologyAttributeRecord>? morphology = null,
        IReadOnlyList<RoadSegmentCarTrafficDirectionAttributeRecord>? car = null)
    {
        return DerivedWegsegmentFlattener.Flatten(
            1, geometry,
            null, null, null, null, null, null,
            morphology ?? [],
            [], [], [], [], [],
            car ?? [],
            [], [],
            null, null,
            DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public void LastToPositionShorterThanGeometry_TrailingCoverageIsExtendedToLength()
    {
        var geometry = Line(100);
        var morphology = new List<RoadSegmentMorphologyAttributeRecord>
        {
            new() { MORF = 1, LBLMORF = "a", VANPOS = 0, TOTPOS = 100 }
        };
        var car = new List<RoadSegmentCarTrafficDirectionAttributeRecord>
        {
            // V1 data: the traffic direction stops short of the actual segment length.
            new() { RICHTING = Forward, VANPOS = 0, TOTPOS = 90 }
        };

        var rows = Flatten(geometry, morphology, car);

        Assert.NotEmpty(rows);
        // The car direction must cover the whole segment: no derived row may lose it near the (stale) end.
        Assert.All(rows, r => Assert.Equal(Forward, r.VERKEERSTYPE_AUTO));
        Assert.Equal(100d, rows.Sum(r => r.GEOMETRIE!.Length), 3);
    }

    [Fact]
    public void LastToPositionLongerThanGeometry_TrailingCoverageIsClampedToLength()
    {
        var geometry = Line(100);
        var car = new List<RoadSegmentCarTrafficDirectionAttributeRecord>
        {
            // V1 data: the traffic direction overshoots the actual segment length.
            new() { RICHTING = Forward, VANPOS = 0, TOTPOS = 120 }
        };

        var rows = Flatten(geometry, car: car);

        Assert.All(rows, r => Assert.Equal(Forward, r.VERKEERSTYPE_AUTO));
        // The flattened geometry is clamped to the actual length (100), never extended to the bogus 120.
        Assert.Equal(100d, rows.Sum(r => r.GEOMETRIE!.Length), 3);
    }

    [Fact]
    public void InternalSplitsPreserved_AndTrailingCoverageExtendedToLength()
    {
        var geometry = Line(100);
        var morphology = new List<RoadSegmentMorphologyAttributeRecord>
        {
            new() { MORF = 1, LBLMORF = "a", VANPOS = 0, TOTPOS = 50 },
            new() { MORF = 2, LBLMORF = "b", VANPOS = 50, TOTPOS = 95 } // trailing ToPosition falls short of length
        };

        var rows = Flatten(geometry, morphology);

        Assert.Equal(2, rows.Count);
        Assert.Equal(1, rows[0].MORF);
        Assert.Equal(2, rows[1].MORF); // trailing morphology still resolved across the extended [50, 100] range
        Assert.Equal(100d, rows.Sum(r => r.GEOMETRIE!.Length), 3);
    }

    [Fact]
    public void SplitJustPastAVertex_ProducesNoRepeatedCoordinates()
    {
        // Attribute positions are stored rounded to centimetres while the vertex's own chainage is not, so a boundary
        // that was digitised on a vertex can end up a few millimetres past it. Cutting there yields both the vertex
        // and the interpolated cut point, which round to the same coordinate - a zero-length segment that SQL Server
        // rejects with "24406: Not valid because curve (n) degenerates to a point".
        var geometry = new LineString([new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(30, 0)]);
        var morphology = new List<RoadSegmentMorphologyAttributeRecord>
        {
            new() { MORF = 1, LBLMORF = "a", VANPOS = 0, TOTPOS = 10.003 },
            new() { MORF = 2, LBLMORF = "b", VANPOS = 10.003, TOTPOS = 30 }
        };

        var rows = Flatten(geometry, morphology);

        Assert.NotEmpty(rows);
        Assert.All(rows, r =>
        {
            var coordinates = r.GEOMETRIE!.Coordinates;
            Assert.True(coordinates.Length >= 2, $"{r.GEOMETRIE} has too few points");
            for (var i = 1; i < coordinates.Length; i++)
            {
                Assert.False(coordinates[i].Equals2D(coordinates[i - 1]),
                    $"{r.GEOMETRIE} repeats the coordinate at index {i}");
            }
        });
        Assert.Equal(30d, rows.Sum(r => r.GEOMETRIE!.Length), 2);
    }
}
