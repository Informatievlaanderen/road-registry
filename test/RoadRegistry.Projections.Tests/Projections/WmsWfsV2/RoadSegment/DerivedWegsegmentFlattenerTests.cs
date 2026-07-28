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
}
