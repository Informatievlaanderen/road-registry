namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.AggregateTests;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

public class RoadSegmentUnflattenerTests
{
    private ITestOutputHelper TestOutputHelper { get; }

    public RoadSegmentUnflattenerTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    //TODO-pr zorg ervoor dat chain zeker in de juiste volgorde is zodat de merge correct kan
    //TODO-pr merge logica zeker unit testen
    [Fact]
    public void With2FlatSegmentsAnd1Schijnknoop_Then1DynamicSegment()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze<RoadSegmentStatusV2>();

        var featureType = fixture.Create<FeatureType>();
        var maxUsedRoadSegmentId = new RoadSegmentId(500);
        var translateContext = new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty);

        translateContext.AddRoadNodeRecords([new RoadNodeFeatureCompareRecord(
            featureType,
            new RecordNumber(1),
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(50, 0),
                Grensknoop = false
            },
            new RoadNodeId(1),
            RecordType.Identical)]);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0)
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0)
        };
        var flatRecords = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>
        {
            new(new RecordNumber(1), flatSegment1),
            new(new RecordNumber(2), flatSegment2)
        };

        // Act
        var records = RoadSegmentUnflattener.Unflatten(
            featureType,
            flatRecords,
            maxUsedRoadSegmentId,
            new OgcFeaturesCache([]),
            translateContext,
            CancellationToken.None);

        // Assert
        TestOutputHelper.WriteLine($"Flat segment1: {JsonConvert.SerializeObject(flatSegment1, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        TestOutputHelper.WriteLine($"Flat segment2: {JsonConvert.SerializeObject(flatSegment2, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        TestOutputHelper.WriteLine(string.Empty);
        TestOutputHelper.WriteLine($"Dynamic segments: {JsonConvert.SerializeObject(records, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");

        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();
        dynamicRecord.Attributes.AccessRestriction!.Values.Should().HaveCount(2);

        dynamicRecord.Attributes.AccessRestriction!.Values[0].Coverage.Should().BeEquivalentTo(new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(50)));
        dynamicRecord.Attributes.AccessRestriction!.Values[0].Value.Should().Be(flatSegment1.AccessRestriction);
        dynamicRecord.Attributes.AccessRestriction!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.AccessRestriction!.Values[1].Coverage.Should().BeEquivalentTo(new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(50), new RoadSegmentPositionV2(100)));
        dynamicRecord.Attributes.AccessRestriction!.Values[1].Value.Should().Be(flatSegment2.AccessRestriction);
        dynamicRecord.Attributes.AccessRestriction!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
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
            fixture.Create<RoadSegmentId>(),
            new OgcFeaturesCache([]),
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None);

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
            fixture.Create<RoadSegmentId>(),
            new OgcFeaturesCache([]),
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None);

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
            fixture.Create<RoadSegmentId>(),
            ogcFeaturesCache,
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None);

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
            fixture.Create<RoadSegmentId>(),
            ogcFeaturesCache,
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None);

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
