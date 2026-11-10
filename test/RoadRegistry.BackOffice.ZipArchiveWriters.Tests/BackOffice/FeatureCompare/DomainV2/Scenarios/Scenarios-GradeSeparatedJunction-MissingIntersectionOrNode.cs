namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Xunit.Abstractions;

// The inwinning refuses two road segments that cross away from a road node with traffic allowed on both and no
// ongelijkgrondse kruising between them. The bijhouding neither refuses nor flags it: filling in traffic types is
// not part of what a GRB dienstverlener is asked to do, and they may leave them unknown (-8).
public class GradeSeparatedJunctionScenarios : FeatureCompareTranslatorScenariosBase
{
    public GradeSeparatedJunctionScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-8)]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNodeAndTrafficIsAllowed_ThenNoProblem(short trafficValue)
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd;
                    roadSegment.AUTOHEEN.Value = trafficValue;
                    roadSegment.AUTOTERUG.Value = trafficValue;
                    roadSegment.FIETSHEEN.Value = trafficValue;
                    roadSegment.FIETSTERUG.Value = trafficValue;
                    roadSegment.VOETGANGER.Value = trafficValue;
                }

                var intersection = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString().Centroid;
                builder.TestData.RoadSegment2ShapeRecord.Geometry = new LineString([
                    new(intersection.X, intersection.Y - 1),
                    new(intersection.X, intersection.Y + 5)
                ]).ToMultiLineString();

                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .Build();

        var act = () => TranslateSucceeds(zipArchive);

        await act.Should().NotThrowAsync<ZipArchiveValidationException>();
    }
}
