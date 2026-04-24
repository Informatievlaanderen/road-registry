namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment;

public class RoadSegmentGeometryHelperTests
{
    [Fact]
    public void DetermineMethod_WithAnyNullMethod_ReturnsNull()
    {
        // Arrange
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 10, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(10, 0, 20, 0), Method: null),
            (Geometry: CreateLineString(20, 0, 30, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        };
        var mergedGeometry = CreateLineString(0, 0, 30, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DetermineMethod_WithAllNullMethods_ReturnsNull()
    {
        // Arrange
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 10, 0), Method: (RoadSegmentGeometryDrawMethodV2?)null),
            (Geometry: CreateLineString(10, 0, 20, 0), Method: null)
        };
        var mergedGeometry = CreateLineString(0, 0, 20, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DetermineMethod_WithIngemetenAt75Percent_ReturnsIngemeten()
    {
        // Arrange - 75% of length is Ingemeten (exactly at threshold)
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 75, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(75, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    [Fact]
    public void DetermineMethod_WithIngemetenAbove75Percent_ReturnsIngemeten()
    {
        // Arrange - 80% of length is Ingemeten (above threshold)
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 80, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(80, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    [Fact]
    public void DetermineMethod_WithIngemetenBelow75Percent_ReturnsIngeschetst()
    {
        // Arrange - 74% of length is Ingemeten (below threshold)
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 74, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(74, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
    }

    [Fact]
    public void DetermineMethod_WithAllIngemeten_ReturnsIngemeten()
    {
        // Arrange - 100% of length is Ingemeten
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 50, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(50, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    [Fact]
    public void DetermineMethod_WithAllIngeschetst_ReturnsIngeschetst()
    {
        // Arrange - 0% of length is Ingemeten
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 50, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst),
            (Geometry: CreateLineString(50, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
    }

    [Fact]
    public void DetermineMethod_WithMultipleIngemetenSegments_SumsLengthsCorrectly()
    {
        // Arrange - 3 Ingemeten segments totaling 76% of length
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 25, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(25, 0, 50, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst),
            (Geometry: CreateLineString(50, 0, 75, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten),
            (Geometry: CreateLineString(75, 0, 102, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten)
        };
        var mergedGeometry = CreateLineString(0, 0, 102, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    [Fact]
    public void DetermineMethod_WithEmptySegmentsList_ThrowsException()
    {
        // Arrange
        var segments = Array.Empty<(MultiLineString Geometry, RoadSegmentGeometryDrawMethodV2? Method)>();
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var act = () => RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DetermineMethod_WithSingleIngemetenSegment_ReturnsIngemeten()
    {
        // Arrange
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingemeten)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    [Fact]
    public void DetermineMethod_WithSingleIngeschetstSegment_ReturnsIngeschetst()
    {
        // Arrange
        var segments = new[]
        {
            (Geometry: CreateLineString(0, 0, 100, 0), Method: RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        };
        var mergedGeometry = CreateLineString(0, 0, 100, 0);

        // Act
        var result = RoadSegmentGeometryHelper.DetermineMethod(segments, mergedGeometry);

        // Assert
        result.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
    }

    private static MultiLineString CreateLineString(double x1, double y1, double x2, double y2)
    {
        var coordinates = new[]
        {
            new Coordinate(x1, y1),
            new Coordinate(x2, y2)
        };
        return new MultiLineString(new[] { new LineString(coordinates) });
    }
}
