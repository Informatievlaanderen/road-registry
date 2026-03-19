namespace RoadRegistry.Tests.Extensions;

using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;
using Xunit;

public class NumericExtensionsTests
{
    [Theory]
    [InlineData(1.0, 1.011, 0.01, false)]
    [InlineData(1.0, 1.010, 0.01, true)]
    [InlineData(1.0, 1.009, 0.01, true)]
    [InlineData(1.0, 1.000, 0.01, true)]
    [InlineData(1.0, 0.999, 0.01, true)]
    [InlineData(1.0, 0.991, 0.01, true)]
    [InlineData(1.0, 0.990, 0.01, true)]
    [InlineData(1.0, 0.989, 0.01, false)]
    public void IsReasonablyEqualTo_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyEqualTo(other, tolerance));
    }

    [Theory]
    [InlineData(1.0, 2.000, 0.01, true)]
    [InlineData(1.0, 1.011, 0.01, true)]
    [InlineData(1.0, 1.010, 0.01, false)]
    [InlineData(1.0, 1.001, 0.01, false)]
    [InlineData(1.0, 1.000, 0.01, false)]
    [InlineData(1.0, 0.999, 0.01, false)]
    [InlineData(1.0, 0.991, 0.01, false)]
    [InlineData(1.0, 0.990, 0.01, false)]
    [InlineData(1.0, 0.989, 0.01, false)]
    public void IsReasonablyLessThan_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyLessThan(other, tolerance));
    }

    [Theory]
    [InlineData(1.0, 2.000, 0.01, true)]
    [InlineData(1.0, 1.011, 0.01, true)]
    [InlineData(1.0, 1.010, 0.01, true)]
    [InlineData(1.0, 1.001, 0.01, true)]
    [InlineData(1.0, 1.000, 0.01, true)]
    [InlineData(1.0, 0.999, 0.01, true)]
    [InlineData(1.0, 0.991, 0.01, true)]
    [InlineData(1.0, 0.990, 0.01, true)]
    [InlineData(1.0, 0.989, 0.01, false)]
    public void IsReasonablyLessOrEqualThan_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyLessOrEqualThan(other, tolerance));
    }

    [Theory]
    [InlineData(2.000, 1.0, 0.01, true)]
    [InlineData(1.011, 1.0, 0.01, true)]
    [InlineData(1.010, 1.0, 0.01, false)]
    [InlineData(1.001, 1.0, 0.01, false)]
    [InlineData(1.000, 1.0, 0.01, false)]
    [InlineData(0.999, 1.0, 0.01, false)]
    [InlineData(0.991, 1.0, 0.01, false)]
    [InlineData(0.990, 1.0, 0.01, false)]
    [InlineData(0.989, 1.0, 0.01, false)]
    public void IsReasonablyGreaterThan_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyGreaterThan(other, tolerance));
    }

    [Theory]
    [InlineData(2.000, 1.0, 0.01, true)]
    [InlineData(1.011, 1.0, 0.01, true)]
    [InlineData(1.010, 1.0, 0.01, true)]
    [InlineData(1.001, 1.0, 0.01, true)]
    [InlineData(1.000, 1.0, 0.01, true)]
    [InlineData(0.999, 1.0, 0.01, true)]
    [InlineData(0.991, 1.0, 0.01, true)]
    [InlineData(0.990, 1.0, 0.01, true)]
    [InlineData(0.989, 1.0, 0.01, false)]
    public void IsReasonablyGreaterOrEqualThan_ReturnsExpectedResult(double value, double other, double tolerance, bool expected)
    {
        Assert.Equal(expected, value.IsReasonablyGreaterOrEqualThan(other, tolerance));
    }

    [Fact]
    public void IsReasonablyEqualTo_RoadSegmentPositionV2_UsesGeometryToleranceV2()
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
        Assert.Equal(new RoadSegmentPositionV2(0.01), new RoadSegmentPositionV2(0.006).RoundToCm());
    }
}
