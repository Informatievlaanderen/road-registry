namespace RoadRegistry.Tests.Extensions;

using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;
using Xunit;

public class NumericExtensionsTests
{
    [Theory]
    [InlineData(1.0, 1.0, 0.001, true)]
    [InlineData(1.0, 1.0009, 0.001, true)]
    [InlineData(1.0, 1.001, 0.001, true)]
    [InlineData(1.0, 1.0011, 0.001, false)]
    [InlineData(1.0, 0.999, 0.001, true)]
    [InlineData(1.0, 0.9989, 0.001, false)]
    [InlineData(5.5, 5.5, 0.01, true)]
    [InlineData(5.5, 5.51, 0.01, false)]
    [InlineData(5.5, 5.49, 0.01, false)]
    public void IsReasonablyEqualTo_Double_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyEqualTo(other, tolerance));
    }

    [Theory]
    [InlineData(1.0, 1.0, 0.001, true)]
    [InlineData(1.0, 1.0009, 0.001, true)]
    [InlineData(1.0, 1.001, 0.001, true)]
    [InlineData(1.0, 1.0011, 0.001, false)]
    [InlineData(1.0, 0.999, 0.001, true)]
    [InlineData(1.0, 0.9989, 0.001, false)]
    public void IsReasonablyEqualTo_Decimal_ReturnsExpectedResult(double valueDouble, double otherDouble, double toleranceDouble, bool expected)
    {
        var value = (decimal)valueDouble;
        var other = (decimal)otherDouble;
        var tolerance = (decimal)toleranceDouble;

        Assert.Equal(expected, value.IsReasonablyEqualTo(other, tolerance));
    }

    [Fact]
    public void IsReasonablyEqualTo_RoadSegmentPosition_UsesGeometryTolerance()
    {
        var position = new RoadSegmentPosition(1.0M);

        Assert.True(position.IsReasonablyEqualTo(1.0));
        Assert.True(position.IsReasonablyEqualTo(1.0 + DefaultTolerances.GeometryTolerance));
        Assert.True(position.IsReasonablyEqualTo(1.0 - DefaultTolerances.GeometryTolerance));
        Assert.False(position.IsReasonablyEqualTo(1.0 + DefaultTolerances.GeometryTolerance + 0.0001));
    }

    [Fact]
    public void IsReasonablyEqualTo_RoadSegmentPositionV2_WithDouble_UsesGeometryToleranceV2()
    {
        var position = new RoadSegmentPositionV2(1.0);

        Assert.True(position.IsReasonablyEqualTo(1.0));
        Assert.True(position.IsReasonablyEqualTo(1.0 + DefaultTolerances.GeometryToleranceV2));
        Assert.True(position.IsReasonablyEqualTo(1.0 - DefaultTolerances.GeometryToleranceV2));
        Assert.False(position.IsReasonablyEqualTo(1.0 + DefaultTolerances.GeometryToleranceV2 + 0.001));
    }

    [Fact]
    public void IsReasonablyEqualTo_RoadSegmentPositionV2_WithOtherPosition_UsesGeometryToleranceV2()
    {
        var position1 = new RoadSegmentPositionV2(1.0);
        var position2 = new RoadSegmentPositionV2(1.0 + DefaultTolerances.GeometryToleranceV2);
        var position3 = new RoadSegmentPositionV2(1.0 + DefaultTolerances.GeometryToleranceV2 + 0.001);

        Assert.True(position1.IsReasonablyEqualTo(position1));
        Assert.True(position1.IsReasonablyEqualTo(position2));
        Assert.False(position1.IsReasonablyEqualTo(position3));
    }

    [Fact]
    public void RoundToCm_RoadSegmentPositionV2_RoundsToTwoDecimals()
    {
        Assert.Equal(new RoadSegmentPositionV2(1.23), new RoadSegmentPositionV2(1.234).RoundToCm());
        Assert.Equal(new RoadSegmentPositionV2(1.24), new RoadSegmentPositionV2(1.235).RoundToCm());
        Assert.Equal(new RoadSegmentPositionV2(1.24), new RoadSegmentPositionV2(1.236).RoundToCm());
        Assert.Equal(new RoadSegmentPositionV2(0.0), new RoadSegmentPositionV2(0.004).RoundToCm());
        Assert.Equal(new RoadSegmentPositionV2(0.01), new RoadSegmentPositionV2(0.005).RoundToCm());
    }

    [Theory]
    [InlineData(1.0, 2.0, 0.5, true)]
    [InlineData(1.0, 2.0, 1.0, false)]
    [InlineData(1.0, 2.0, 1.1, false)]
    [InlineData(1.0, 1.5, 0.49, true)]
    [InlineData(1.0, 1.5, 0.5, false)]
    [InlineData(5.0, 3.0, 0.5, false)]
    public void IsReasonablyLessThan_Double_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyLessThan(other, tolerance));
    }

    [Theory]
    [InlineData(1.0, 2.0, 0.5, true)]
    [InlineData(1.0, 2.0, 1.0, false)]
    [InlineData(1.0, 1.5, 0.49, true)]
    [InlineData(5.0, 3.0, 0.5, false)]
    public void IsReasonablyLessThan_Decimal_ReturnsExpectedResult(double valueDouble, double otherDouble, double toleranceDouble, bool expected)
    {
        var value = (decimal)valueDouble;
        var other = (decimal)otherDouble;
        var tolerance = (decimal)toleranceDouble;

        Assert.Equal(expected, value.IsReasonablyLessThan(other, tolerance));
    }

    [Theory]
    [InlineData(1.0, 2.0, 0.5, true)]
    [InlineData(1.0, 2.0, 1.0, true)]
    [InlineData(1.0, 1.0, 1.0, true)]
    [InlineData(1.0, 2.0, 1.1, false)]
    [InlineData(1.0, 1.5, 0.5, true)]
    [InlineData(1.0, 1.5, 0.49, false)]
    [InlineData(5.0, 3.0, 0.5, false)]
    public void IsReasonablyLessOrEqualThan_Double_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyLessOrEqualThan(other, tolerance));
    }

    [Theory]
    [InlineData(2.0, 1.0, 0.5, true)]
    [InlineData(2.0, 1.0, 1.0, false)]
    [InlineData(2.0, 1.0, 1.1, false)]
    [InlineData(1.5, 1.0, 0.49, true)]
    [InlineData(1.5, 1.0, 0.5, false)]
    [InlineData(3.0, 5.0, 0.5, false)]
    public void IsReasonablyGreaterThan_Double_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyGreaterThan(other, tolerance));
    }

    [Theory]
    [InlineData(2.0, 1.0, 0.5, true)]
    [InlineData(2.0, 1.0, 1.0, false)]
    [InlineData(1.5, 1.0, 0.49, true)]
    [InlineData(3.0, 5.0, 0.5, false)]
    public void IsReasonablyGreaterThan_Decimal_ReturnsExpectedResult(double valueDouble, double otherDouble, double toleranceDouble, bool expected)
    {
        var value = (decimal)valueDouble;
        var other = (decimal)otherDouble;
        var tolerance = (decimal)toleranceDouble;

        Assert.Equal(expected, value.IsReasonablyGreaterThan(other, tolerance));
    }

    [Theory]
    [InlineData(2.0, 1.0, 0.5, true)]
    [InlineData(2.0, 1.0, 1.0, true)]
    [InlineData(2.0, 2.0, 1.0, true)]
    [InlineData(2.0, 1.0, 1.1, false)]
    [InlineData(1.5, 1.0, 0.5, true)]
    [InlineData(1.5, 1.0, 0.49, false)]
    [InlineData(3.0, 5.0, 0.5, false)]
    public void IsReasonablyGreaterOrEqualThan_Double_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyGreaterOrEqualThan(other, tolerance));
    }
}
