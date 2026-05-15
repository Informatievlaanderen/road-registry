namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.RoadSegmentUnflattenerTests;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

public class UnflattenByRoadSegmentIdTests
{
    private ITestOutputHelper TestOutputHelper { get; }

    public UnflattenByRoadSegmentIdTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Fact]
    public void With2FlatSegments_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            TempId = new RoadSegmentTempId(1),
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            AccessRestriction = RoadSegmentAccessRestrictionV2.OpenbareWeg
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            TempId = new RoadSegmentTempId(2),
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0),
            AccessRestriction = RoadSegmentAccessRestrictionV2.PrivateWeg
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2
        };

        // Act
        var records = Unflatten(flatSegments);

        // Assert
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

    private List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(IReadOnlyList<RoadSegmentFeatureCompareWithFlatAttributes> flatSegments)
    {
        var flatRecords = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
        for (var i = 0; i < flatSegments.Count; i++)
        {
            flatRecords.Add(new(new RecordNumber(i + 1), flatSegments[i]));
            TestOutputHelper.WriteLine($"Flat segment {flatSegments[i].RoadSegmentId}: {JsonConvert.SerializeObject(flatSegments[i], Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        }

        var translateContext = new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty);

        var unflattenedRecords = RoadSegmentUnflattener.UnflattenByRoadSegmentId(
            flatRecords,
            new OgcFeaturesCache([]),
            translateContext);

        TestOutputHelper.WriteLine(string.Empty);
        TestOutputHelper.WriteLine("-->");
        TestOutputHelper.WriteLine($"Unflatten result: {JsonConvert.SerializeObject(unflattenedRecords, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");


        return unflattenedRecords;
    }

    private static MultiLineString BuildRoadSegmentGeometry(int x1, int y1, int x2, int y2)
    {
        return BuildRoadSegmentGeometry(new Coordinate(x1, y1), new Coordinate(x2, y2));
    }

    private static MultiLineString BuildRoadSegmentGeometry(Coordinate start, Coordinate end)
    {
        return BuildRoadSegmentGeometry([start, end]);
    }

    private static MultiLineString BuildRoadSegmentGeometry(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates)]);
    }
}
