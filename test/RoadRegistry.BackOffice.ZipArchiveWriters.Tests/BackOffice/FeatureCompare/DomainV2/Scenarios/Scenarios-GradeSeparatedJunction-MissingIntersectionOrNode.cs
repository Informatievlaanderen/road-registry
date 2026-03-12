namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;

public partial class GradeSeparatedJunctionScenarios
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNodeAndCarsAreAllowed_ThenError(bool autoHeen, bool autoTerug)
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(builder);

                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.AUTOHEEN.Value = autoHeen.ToDbaseShortValue();
                    roadSegment.AUTOTERUG.Value = autoTerug.ToDbaseShortValue();
                }
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenCarsAreAllowed));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNodeAndBikesAreAllowed_ThenError(bool fietsHeen, bool fietsTerug)
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(builder);

                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.FIETSHEEN.Value = fietsHeen.ToDbaseShortValue();
                    roadSegment.FIETSTERUG.Value = fietsTerug.ToDbaseShortValue();
                }
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenBikesAreAllowed));
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNodeAndPedestriansAreAllowed_ThenError()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(builder);

                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.VOETGANGER.Value = true.ToDbaseShortValue();
                }
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenPedestriansAreAllowed));
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNodeAndAllAccessAreAllowed_ThenAllAccessErrors()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(builder);

                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.AUTOHEEN.Value = true.ToDbaseShortValue();
                    roadSegment.FIETSHEEN.Value = true.ToDbaseShortValue();
                    roadSegment.VOETGANGER.Value = true.ToDbaseShortValue();
                }
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenCarsAreAllowed));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenBikesAreAllowed));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenPedestriansAreAllowed));
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNodeAndNoneAccessAreAllowed_ThenNoProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(builder);

                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.AUTOHEEN.Value = false.ToDbaseShortValue();
                    roadSegment.FIETSHEEN.Value = false.ToDbaseShortValue();
                    roadSegment.VOETGANGER.Value = false.ToDbaseShortValue();
                }
            })
            .Build();

        await TranslateSucceeds(zipArchive);
    }

    private static void ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(ExtractsZipArchiveExtractDataSetBuilder builder)
    {
        foreach (var roadSegmentDbaseRecord in builder.DataSet.RoadSegmentDbaseRecords)
        {
            roadSegmentDbaseRecord.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd;

            roadSegmentDbaseRecord.AUTOHEEN.Value = 0;
            roadSegmentDbaseRecord.AUTOTERUG.Value = 0;
            roadSegmentDbaseRecord.FIETSHEEN.Value = 0;
            roadSegmentDbaseRecord.FIETSTERUG.Value = 0;
            roadSegmentDbaseRecord.VOETGANGER.Value = 0;
        }

        var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();

        var intersection = roadSegment1Geometry.Centroid;

        var roadSegment2Geometry = new LineString([
            new(intersection.X, intersection.Y - 1),
            new(intersection.X, intersection.Y + 5)
        ]);
        builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();

        builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
    }

    [Fact]
    public async Task IntersectingNotGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionShouldNotGiveProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunctionOrNode(builder);

                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentStatusV2.Gerealiseerd);
                builder.TestData.RoadSegment2DbaseRecord.STATUS.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentStatusV2.Gerealiseerd);
            })
            .Build();

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task IntersectingRoadSegmentsInA_TShape_WithoutGradeSeparatedJunctionShouldNotGiveProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture =>
            {
                fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);
            })
            .WithChange((builder, context) =>
            {
                var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();

                var intersection = roadSegment1Geometry.Centroid;

                var roadSegment2Geometry = new LineString([
                    new(intersection.X, intersection.Y),
                    new(intersection.X, intersection.Y + 5)
                ]);
                builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();

                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .Build();

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNode_AndSegmentContainsMultipleFlatSegments_AndIntersectionIsInMiddleOfFlatSegment_ThenCorrectTempIdsAreUsedInProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture =>
            {
                fixture.CustomizeUniqueInteger();
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.Clear();

                var segment1StartPoint = new Point(601000, 601050);
                var segment1MiddlePoint1 = new Point(601010, 601050);
                var segment1MiddlePoint2 = new Point(601020, 601050);
                var segment1EndPoint = new Point(601030, 601050);

                var segment2StartPoint = new Point(601015, 601040);
                var segment2EndPoint = new Point(601015, 601060);

                builder.DataSet.RoadSegmentShapeRecords.AddRange([
                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment1StartPoint, segment1MiddlePoint1)),
                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment1MiddlePoint1, segment1MiddlePoint2)),
                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment1MiddlePoint2, segment1EndPoint)),

                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment2StartPoint, segment2EndPoint))
                ]);

                builder.DataSet.RoadSegmentDbaseRecords.AddRange([
                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 11),
                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 12),
                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 13),

                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 21)
                ]);
                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd.Translation.Identifier;
                    roadSegment.AUTOHEEN.Value = true.ToDbaseShortValue();
                }

                builder.DataSet.RoadNodeShapeRecords.AddRange([
                    builder.CreateRoadNodeShapeRecord(segment1StartPoint),
                    builder.CreateRoadNodeShapeRecord(segment1MiddlePoint1),
                    builder.CreateRoadNodeShapeRecord(segment1MiddlePoint2),
                    builder.CreateRoadNodeShapeRecord(segment1EndPoint),

                    builder.CreateRoadNodeShapeRecord(segment2StartPoint),
                    builder.CreateRoadNodeShapeRecord(segment2EndPoint)
                ]);
                builder.DataSet.RoadNodeDbaseRecords.AddRange([
                    builder.CreateRoadNodeDbaseRecord(), //Eindknoop
                    builder.CreateRoadNodeDbaseRecord(record => record.GRENSKNOOP.Value = 0), //Schijnknoop
                    builder.CreateRoadNodeDbaseRecord(record => record.GRENSKNOOP.Value = 0), //Schijnknoop
                    builder.CreateRoadNodeDbaseRecord(), //Eindknoop

                    builder.CreateRoadNodeDbaseRecord(),
                    builder.CreateRoadNodeDbaseRecord(),
                ]);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenCarsAreAllowed)
                                          && x.GetParameterValue("Wegsegment1TempIds") == "12"
                                          && x.GetParameterValue("Wegsegment2TempIds") == "21");
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutJunctionOrNode_AndSegmentContainsMultipleFlatSegments_AndIntersectionIsInBetween2FlatSegments_ThenCorrectTempIdsAreUsedInProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture =>
            {
                fixture.CustomizeUniqueInteger();
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.Clear();

                var segment1StartPoint = new Point(601000, 601050);
                var segment1MiddlePoint1 = new Point(601010, 601050);
                var segment1MiddlePoint2 = new Point(601020, 601050);
                var segment1EndPoint = new Point(601030, 601050);

                var segment2StartPoint = new Point(601010.001, 601040);
                var segment2EndPoint = new Point(601010.001, 601060);

                builder.DataSet.RoadSegmentShapeRecords.AddRange([
                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment1StartPoint, segment1MiddlePoint1)),
                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment1MiddlePoint1, segment1MiddlePoint2)),
                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment1MiddlePoint2, segment1EndPoint)),

                    builder.CreateRoadSegmentShapeRecord(BuildRoadSegmentGeometry(segment2StartPoint, segment2EndPoint))
                ]);

                builder.DataSet.RoadSegmentDbaseRecords.AddRange([
                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 11),
                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 12),
                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 13),

                    builder.CreateRoadSegmentDbaseRecord(record => record.WS_TEMPID.Value = 21)
                ]);
                foreach (var roadSegment in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegment.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd.Translation.Identifier;
                    roadSegment.AUTOHEEN.Value = true.ToDbaseShortValue();
                }

                builder.DataSet.RoadNodeShapeRecords.AddRange([
                    builder.CreateRoadNodeShapeRecord(segment1StartPoint),
                    builder.CreateRoadNodeShapeRecord(segment1MiddlePoint1),
                    builder.CreateRoadNodeShapeRecord(segment1MiddlePoint2),
                    builder.CreateRoadNodeShapeRecord(segment1EndPoint),

                    builder.CreateRoadNodeShapeRecord(segment2StartPoint),
                    builder.CreateRoadNodeShapeRecord(segment2EndPoint)
                ]);
                builder.DataSet.RoadNodeDbaseRecords.AddRange([
                    builder.CreateRoadNodeDbaseRecord(), //Eindknoop
                    builder.CreateRoadNodeDbaseRecord(record => record.GRENSKNOOP.Value = 0), //Schijnknoop
                    builder.CreateRoadNodeDbaseRecord(record => record.GRENSKNOOP.Value = 0), //Schijnknoop
                    builder.CreateRoadNodeDbaseRecord(), //Eindknoop

                    builder.CreateRoadNodeDbaseRecord(),
                    builder.CreateRoadNodeDbaseRecord(),
                ]);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        ex.Problems.Should().Contain(x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionOrRoadNodeMissingWhenCarsAreAllowed)
                                          && x.GetParameterValue("Wegsegment1TempIds") == "11,12"
                                          && x.GetParameterValue("Wegsegment2TempIds") == "21");
    }

    private static RoadSegmentGeometry BuildRoadSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }
}
