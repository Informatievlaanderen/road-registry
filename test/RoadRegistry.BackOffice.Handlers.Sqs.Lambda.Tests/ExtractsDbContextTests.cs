namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using FluentAssertions;
using NetTopologySuite.Geometries;
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
}
