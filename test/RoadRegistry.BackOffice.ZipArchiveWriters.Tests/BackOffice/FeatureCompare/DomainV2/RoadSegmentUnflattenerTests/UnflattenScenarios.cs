namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.RoadSegmentUnflattenerTests;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
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

public class UnflattenScenarios
{
    private ITestOutputHelper TestOutputHelper { get; }

    public UnflattenScenarios(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Fact]
    public void With2FlatSegmentsAnd1Schijnknoop_Then1DynamicSegment()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze<RoadSegmentStatusV2>();

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0)
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0)
        };
        var flatSegments = new []
        {
            flatSegment1,
            flatSegment2
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(50, 0),
                Grensknoop = false
            }
        };

        // Act
        var records = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);

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

    [Fact]
    public void WhenSelfIntersectsAndNoNodeNearExpectedLocation_ThenProblem()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentStatusV2>();

        var scenario = @"
2=LINESTRING (0 0, 10 0, 10 -30)
3=POINT (10 -30)
4=LINESTRING (0 0, 0 -10, 20 -10)
5=POINT (20 -10)
";

        // Act
        var act = () => Unflatten(fixture, scenario);

        // Assert
        act.Should().Throw<NotImplementedException>(); //TODO-pr should return throw problem
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocation_Then2DynamicSegments()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentStatusV2>();

        //TODO-pr test variants met omgekeerde richtingen van geometries, voor zowel de 1e als de 2e + nakijken of de passende attributen correct worden mee gecorrigeerd
        var scenario = @"
1=POINT (0 0)
2=LINESTRING (0 0, 10 0, 10 -30)
3=POINT (10 -30)
4=LINESTRING (0 0, 0 -10, 20 -10)
5=POINT (20 -10)
";

        // Act
        var records = Unflatten(fixture, scenario);

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.RoadSegmentId.Should().Be(new RoadSegmentId(2));
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 10 0, 10 -30))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.RoadSegmentId.Should().Be(new RoadSegmentId(4));
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((20 -10, 0 -10, 0 0))");
    }

    [Fact]
    public void WhenSameStartEndNodeAndNoNodeInBetween_ThenProblem()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void WhenSameStartEndNodeAndNodeFoundInBetween_Then2DynamicSegments()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void WhenMultipleIntersectionsWithSameOtherGeometryAndNoNodeInBetween_ThenProblem()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void WhenMultipleIntersectionsWithSameOtherGeometryAndNodeFoundInBetween_Then2DynamicSegments()
    {
        throw new NotImplementedException();
    }

    //TODO-pr tests voor de DetectAndFixInvalidGeometries scenarios: edge cases

    private List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(
        IFixture fixture,
        string scenarioWkt)
    {
        var scenarioData = scenarioWkt
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
            {
                var line = x.Split('=');
                var id = int.Parse(line[0]);
                var geometry = new WKTReader().Read(line[1]);
                return (id, geometry);
            })
            .ToArray();

        var nodes = scenarioData
            .Where(x => x.geometry is Point)
            .Select(x => new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new(x.id),
                Geometry = (Point)x.geometry
            })
            .ToList();

        var flatSegments = scenarioData
            .Where(x => x.geometry is LineString)
            .Select(x => fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
            {
                RoadSegmentId = new(x.id),
                Geometry = ((LineString)x.geometry).ToMultiLineString()
            })
            .ToList();

        return Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(
        FeatureType featureType,
        IReadOnlyList<RoadSegmentFeatureCompareWithFlatAttributes> flatSegments,
        IReadOnlyList<RoadNodeFeatureCompareAttributes> roadNodes)
    {
        var flatRecords = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
        for (var i = 0; i < flatSegments.Count; i++)
        {
            flatRecords.Add(new(new RecordNumber(i + 1), flatSegments[i]));
            TestOutputHelper.WriteLine($"Flat segment {flatSegments[i].RoadSegmentId}: {JsonConvert.SerializeObject(flatSegments[i], Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        }

        var roadSegmentIdProver = new NextRoadSegmentIdProvider(new RoadSegmentId(10000));
        var translateContext = new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty);

        for (var i = 0; i < roadNodes.Count; i++)
        {
            translateContext.AddRoadNodeRecords([new RoadNodeFeatureCompareRecord(
                featureType,
                new RecordNumber(i + 1),
                roadNodes[i],
                roadNodes[i].RoadNodeId,
                RecordType.Identical)]);
            TestOutputHelper.WriteLine($"Node {roadNodes[i].RoadNodeId}: {JsonConvert.SerializeObject(roadNodes[i], Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        }

        var unflattenedRecords = RoadSegmentUnflattener.Unflatten(
            featureType,
            flatRecords,
            roadSegmentIdProver,
            new OgcFeaturesCache([]),
            translateContext,
            CancellationToken.None);

        TestOutputHelper.WriteLine(string.Empty);
        TestOutputHelper.WriteLine("-->");
        TestOutputHelper.WriteLine($"Dynamic segments: {JsonConvert.SerializeObject(unflattenedRecords, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");

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
