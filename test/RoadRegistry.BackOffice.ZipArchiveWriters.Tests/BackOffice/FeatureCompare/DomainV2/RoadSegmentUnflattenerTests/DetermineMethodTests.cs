namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.RoadSegmentUnflattenerTests;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.AggregateTests;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

public class DetermineMethodTests
{
    private ITestOutputHelper TestOutputHelper { get; }

    public DetermineMethodTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WithMethodNotNull_ThenMethodIsKept()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var flatSegment = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Method = fixture.Create<RoadSegmentGeometryDrawMethodV2>()
        };

        // Act
        var records = RoadSegmentUnflattener.Unflatten(
            fixture.Create<FeatureType>(),
            [new(new RecordNumber(1), flatSegment)],
            [],
            fixture.Create<NextRoadSegmentIdProvider>(),
            new OgcFeaturesCache([]),
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None).RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.Method!.Should().Be(flatSegment.Method);
    }

    [Fact]
    public void WithMethodNullAndStatusGepland_ThenIngeschetst()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var flatSegment = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Method = null,
            Status = RoadSegmentStatusV2.Gepland
        };

        // Act
        var records = RoadSegmentUnflattener.Unflatten(
            fixture.Create<FeatureType>(),
            [new(new RecordNumber(1), flatSegment)],
            [],
            fixture.Create<NextRoadSegmentIdProvider>(),
            new OgcFeaturesCache([]),
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None).RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.Method!.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
    }

    [Fact]
    public void WithMethodNullAndStatusNotGeplandAndNoOgcOverlap_ThenIngeschetst()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var flatSegment = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            Method = null,
            Status = fixture.CreateWhichIsDifferentThan(RoadSegmentStatusV2.Gepland)
        };

        var ogcFeaturesCache = new OgcFeaturesCache([]);

        // Act
        var records = RoadSegmentUnflattener.Unflatten(
            fixture.Create<FeatureType>(),
            [new(new RecordNumber(1), flatSegment)],
            [],
            fixture.Create<NextRoadSegmentIdProvider>(),
            ogcFeaturesCache,
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None).RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.Method!.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
    }

    [Fact]
    public void WithMethodNullAndStatusNotGeplandAndOgcOverlap_ThenIngemeten()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var flatSegment = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            Method = null,
            Status = fixture.CreateWhichIsDifferentThan(RoadSegmentStatusV2.Gepland)
        };

        var ogcFeaturesCache = new OgcFeaturesCache([new OgcFeature(fixture.Create<string>(), null, flatSegment.Geometry.Buffer(100), null)]);

        // Act
        var records = RoadSegmentUnflattener.Unflatten(
            fixture.Create<FeatureType>(),
            [new(new RecordNumber(1), flatSegment)],
            [],
            fixture.Create<NextRoadSegmentIdProvider>(),
            ogcFeaturesCache,
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None).RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.Method!.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    private static MultiLineString BuildRoadSegmentGeometry(int x1, int y1, int x2, int y2)
    {
        return BuildRoadSegmentGeometry(new Point(x1, y1), new Point(x2, y2));
    }

    private static MultiLineString BuildRoadSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])]);
    }
}
