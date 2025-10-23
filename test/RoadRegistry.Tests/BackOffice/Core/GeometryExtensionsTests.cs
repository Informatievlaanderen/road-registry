namespace RoadRegistry.Tests.BackOffice.Core;

using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Core;
using Point = NetTopologySuite.Geometries.Point;

public class GeometryExtensionsTests
{
    private static readonly VerificationContextTolerances Tolerances = VerificationContextTolerances.Default;

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, 0.0009)]
    [InlineData(true, 0.001)]
    [InlineData(false, 0.0011)]
    public void Point_IsReasonablyEqualTo(bool expected, double distance)
    {
        var p1 = new Point(0, 0);
        var p2 = new Point(p1.X + distance, 0);
        var isEqual = p1.IsReasonablyEqualTo(p2, Tolerances);

        Assert.Equal(expected, isEqual);
    }

    [Theory]
    [InlineData(false, 0, -0.0011)]
    [InlineData(true, 0, -0.001)]
    [InlineData(true, 0, -0.0001)]
    [InlineData(true, 0, 0)]
    [InlineData(true, 0, 0.0001)]
    [InlineData(true, 0, 0.001)]
    [InlineData(false, 0, 0.0011)]
    public void IsReasonablyEqualTo(bool expected, decimal value1, decimal value2)
    {
        Assert.Equal(expected, value1.IsReasonablyEqualTo(value2, Tolerances));
    }

    [Theory]
    [InlineData(false, 0, 0)]
    [InlineData(false, 0, 0.0001)]
    [InlineData(false, 0, 0.001)]
    [InlineData(true, 0, 0.0011)]
    public void IsReasonablyLessThan(bool expected, decimal value1, decimal value2)
    {
        Assert.Equal(expected, value1.IsReasonablyLessThan(value2, Tolerances));
    }

    [Theory]
    [InlineData(false, 0, 0)]
    [InlineData(false, 0.0001, 0)]
    [InlineData(false, 0.001, 0)]
    [InlineData(true, 0.0011, 0)]
    public void IsReasonablyGreaterThan(bool expected, decimal value1, decimal value2)
    {
        Assert.Equal(expected, value1.IsReasonablyGreaterThan(value2, Tolerances));
    }
}
