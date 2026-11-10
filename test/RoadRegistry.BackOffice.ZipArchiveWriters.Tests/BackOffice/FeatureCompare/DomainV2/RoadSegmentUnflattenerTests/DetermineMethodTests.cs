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
        var records = RoadSegmentUnflattener.UnflattenByTopology(
            fixture.Create<FeatureType>(),
            [new(new RecordNumber(1), flatSegment)],
            [],
            fixture.Create<NextRoadSegmentIdProvider>(),
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None).RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.Method!.Should().Be(flatSegment.Method);
    }

    // The reader refuses a bijhouding record without a METHODE, so one reaching the unflattener is a programming
    // error. Unlike the inwinning, there is no GRB to fall back on to guess it.
    [Fact]
    public void WithMethodNull_ThenInvalidOperationException()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var flatSegment = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Method = null
        };

        // Act
        var act = () => RoadSegmentUnflattener.UnflattenByTopology(
            fixture.Create<FeatureType>(),
            [new(new RecordNumber(1), flatSegment)],
            [],
            fixture.Create<NextRoadSegmentIdProvider>(),
            new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty),
            CancellationToken.None);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
