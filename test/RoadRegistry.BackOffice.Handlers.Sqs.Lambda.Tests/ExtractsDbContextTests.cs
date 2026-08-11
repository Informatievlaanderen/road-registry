namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using FluentAssertions;
using NetTopologySuite.Geometries;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using RoadRegistry.BackOffice;
using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;
using RoadRegistry.Extracts.Schema;

public class ExtractsDbContextTests
{
    private static ExtractsDbContext CreateDbContext() =>
        new FakeExtractsDbContextFactory().CreateDbContext();

    private static MultiLineString LineInsideSquare(double cx, double cy, double len) =>
        new([new LineString([new Coordinate(cx - len / 2, cy), new Coordinate(cx + len / 2, cy)])]);

    private static Geometry Square(double cx, double cy, double half) =>
        new Polygon(new LinearRing([
            new Coordinate(cx - half, cy - half),
            new Coordinate(cx + half, cy - half),
            new Coordinate(cx + half, cy + half),
            new Coordinate(cx - half, cy + half),
            new Coordinate(cx - half, cy - half)
        ]));

    private static Inwinningszone Zone(string nisCode, Geometry contour, bool completed) =>
        new()
        {
            NisCode = nisCode,
            Operator = "op",
            DownloadId = Guid.NewGuid(),
            Contour = contour,
            Completed = completed
        };

    [Fact]
    public async Task WhenNoZones_ReturnsFalse()
    {
        var db = CreateDbContext();
        var geometry = LineInsideSquare(0, 0, 2);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenNoCompletedZones_ReturnsFalse()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", Square(0, 0, 10), completed: false));
        await db.SaveChangesAsync();

        var geometry = LineInsideSquare(0, 0, 2);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenGeometryCompletelyInsideCompletedZone_ReturnsTrue()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", Square(0, 0, 10), completed: true));
        await db.SaveChangesAsync();

        var geometry = LineInsideSquare(0, 0, 2);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenGeometryPartiallyOutsideCompletedZone_ReturnsFalse()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", Square(0, 0, 10), completed: true));
        await db.SaveChangesAsync();

        // line from x=-20 to x=20, extends well outside the zone [-10,10]
        var geometry = LineInsideSquare(0, 0, 40);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenGeometryCompletelyInsideUnionOfTwoAdjacentCompletedZones_ReturnsTrue()
    {
        var db = CreateDbContext();
        // Zone A covers x in [-10,0], Zone B covers x in [0,10] — together they span [-10,10]
        db.Inwinningszones.Add(Zone("11001", Square(-5, 0, 5), completed: true));
        db.Inwinningszones.Add(Zone("11002", Square(5, 0, 5), completed: true));
        await db.SaveChangesAsync();

        // line from x=-4 to x=4, entirely within the union
        var geometry = LineInsideSquare(0, 0, 8);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenCompletedZoneExistsButGeometryIsOutside_ReturnsFalse()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", Square(100, 100, 10), completed: true));
        await db.SaveChangesAsync();

        var geometry = LineInsideSquare(0, 0, 2);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeFalse();
    }

    // Realistic Flemish coordinates: the two reference systems only line up over Belgium, so a square around the
    // origin would not survive the round trip meaningfully.
    private const double Lambert08X = 217368.75;
    private const double Lambert08Y = 181577.02;

    private static Geometry SquareLambert08(double cx, double cy, double half) =>
        GeometryExtensions.WithSrid(Square(cx, cy, half), WellknownSrids.Lambert08);

    private static MultiLineString LineLambert08(double cx, double cy, double len) =>
        GeometryExtensions.WithSrid(LineInsideSquare(cx, cy, len), WellknownSrids.Lambert08);

    // Both sides are put in Lambert 2008 before being compared, whatever they arrive in. Covers and Intersects
    // compare raw coordinates and never look at the SRID, so without that a road and the zone it lies in read as a
    // hundred kilometres apart and every road passes.

    [Fact]
    public async Task WhenTheZoneIsLambert72AndTheGeometryLambert08_ThenTheyAreComparedInTheSameReferenceSystem()
    {
        // A zone that was stored in Lambert 72 is converted before the comparison; the road segment already is in
        // Lambert 2008 and is left alone.
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500).TransformFromLambert08To72(), completed: true));
        await db.SaveChangesAsync();

        var geometry = LineLambert08(Lambert08X, Lambert08Y, 100);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WhenTheZoneIsLambert72AndTheLambert08GeometryLiesOutsideIt_ReturnsFalse()
    {
        // The conversion must not make everything match either: a road 5km away is still outside.
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500).TransformFromLambert08To72(), completed: true));
        await db.SaveChangesAsync();

        var geometry = LineLambert08(Lambert08X + 5000, Lambert08Y, 100);

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WhenTheZoneAndTheGeometryAreBothLambert72_ThenBothAreConverted()
    {
        // Neither side is in the reference system the comparison happens in, so both are transformed - and they
        // still line up afterwards.
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500).TransformFromLambert08To72(), completed: true));
        await db.SaveChangesAsync();

        var geometry = LineLambert08(Lambert08X, Lambert08Y, 100).TransformFromLambert08To72();

        var result = await db.IsCompletelyWithinCompletedInwinningszone(geometry, CancellationToken.None);

        result.Should().BeTrue();
    }

    // CheckWhichOverlapWithInwinningszone normalises the same way: zones and road segments alike end up in
    // Lambert 2008 before being asked whether they intersect.

    private static readonly RoadSegmentId TemporaryId = new(1);

    [Fact]
    public async Task WhenTheZoneIsLambert08AndTheRoadSegmentLambert72_ThenTheOverlapIsStillFound()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500), completed: false));
        await db.SaveChangesAsync();

        // The same road, stated in Lambert 72 - which is what the v1 outline endpoints hand over.
        var geometry = LineLambert08(Lambert08X, Lambert08Y, 100).TransformFromLambert08To72();

        var result = await db.CheckWhichOverlapWithInwinningszone([(geometry, TemporaryId)], CancellationToken.None);

        result.Should().BeEquivalentTo([TemporaryId]);
    }

    [Fact]
    public async Task WhenTheLambert72RoadSegmentLiesOutsideTheZone_ThenNoOverlapIsFound()
    {
        // Converted just the same, but genuinely somewhere else: the conversion must not make everything overlap.
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500), completed: false));
        await db.SaveChangesAsync();

        var geometry = LineLambert08(Lambert08X + 5000, Lambert08Y, 100).TransformFromLambert08To72();

        var result = await db.CheckWhichOverlapWithInwinningszone([(geometry, TemporaryId)], CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenTheZoneAndTheRoadSegmentAreBothLambert08_ThenNothingIsConverted()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500), completed: false));
        await db.SaveChangesAsync();

        var geometry = LineLambert08(Lambert08X, Lambert08Y, 100);

        var result = await db.CheckWhichOverlapWithInwinningszone([(geometry, TemporaryId)], CancellationToken.None);

        result.Should().BeEquivalentTo([TemporaryId]);
    }

    [Fact]
    public async Task WhenOnlyOneOfTheRoadSegmentsOverlaps_ThenOnlyThatOneIsReturned()
    {
        var db = CreateDbContext();
        db.Inwinningszones.Add(Zone("11001", SquareLambert08(Lambert08X, Lambert08Y, 500), completed: false));
        await db.SaveChangesAsync();

        var inside = LineLambert08(Lambert08X, Lambert08Y, 100).TransformFromLambert08To72();
        var outside = LineLambert08(Lambert08X + 5000, Lambert08Y, 100).TransformFromLambert08To72();
        var outsideId = new RoadSegmentId(2);

        var result = await db.CheckWhichOverlapWithInwinningszone(
            [(inside, TemporaryId), (outside, outsideId)], CancellationToken.None);

        result.Should().BeEquivalentTo([TemporaryId]);
    }

    [Fact]
    public async Task WhenThereAreNoZones_ThenNoOverlapIsFound()
    {
        var db = CreateDbContext();

        var geometry = LineLambert08(Lambert08X, Lambert08Y, 100).TransformFromLambert08To72();

        var result = await db.CheckWhichOverlapWithInwinningszone([(geometry, TemporaryId)], CancellationToken.None);

        result.Should().BeEmpty();
    }
}
