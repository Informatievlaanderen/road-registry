namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.ZipArchiveWriters;

public class GewestGrensTests
{
    [Fact]
    public void WithLambert72PointFarFromBorder_ThenFalse()
    {
        var pointFarFromBorder = new Point(688210, 737249) { SRID = 3812 };

        var isClose = GewestGrens.IsCloseToBorder(pointFarFromBorder, 1);
        isClose.Should().BeFalse();
    }

    [Fact]
    public void WithLambert72PointCloseToBorder_ThenTrue()
    {
        var pointFarFromBorder = new Point(688213, 737249) { SRID = 3812 };

        var isClose = GewestGrens.IsCloseToBorder(pointFarFromBorder, 1);
        isClose.Should().BeTrue();
    }

    [Fact]
    public void WithLambert72Point_ThenException()
    {
        var pointFarFromBorder = new Point(0, 0) { SRID = 31370 };

        var act = () => GewestGrens.IsCloseToBorder(pointFarFromBorder, 1);
        act.Should().Throw<ArgumentException>();
    }
}
