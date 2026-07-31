namespace RoadRegistry.Projections.Tests.Projections.Pbs.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema.Records;
using Xunit;

public class DerivedWegsegmentFlattenerTests
{
    private static List<DerivedRoadSegmentRecord> Flatten(
        Geometry geometry,
        IReadOnlyList<RoadSegmentMorphologyAttributeRecord> morphology)
    {
        return DerivedWegsegmentFlattener.Flatten(
            1, geometry,
            null, null, null, null, null, null,
            morphology,
            [], [], [], [], [], [], [], [],
            null, null);
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
