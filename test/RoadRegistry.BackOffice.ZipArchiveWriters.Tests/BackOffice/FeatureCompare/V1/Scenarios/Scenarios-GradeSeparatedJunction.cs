namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V1.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare.V1;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;

public class GradeSeparatedJunctionScenarios : FeatureCompareTranslatorScenariosBase
{
    public GradeSeparatedJunctionScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task RemovedRoadSegmentShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment1DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment1ShapeRecord }.ToList();

                builder.DataSet.LaneDbaseRecords = new[] { builder.TestData.RoadSegment1LaneDbaseRecord }.ToList();
                builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment1SurfaceDbaseRecord }.ToList();
                builder.DataSet.WidthDbaseRecords = new[] { builder.TestData.RoadSegment1WidthDbaseRecord }.ToList();
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange));
    }

    [Fact]
    public async Task EqualLowerAndUpperShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value;
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment));
    }

    [Fact]
    public async Task UnknownRoadSegmentShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value);

                builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value,
                    builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange));
    }

    [Fact]
    public async Task IntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunctionShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange(ConfigureIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction)
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionMissing));
    }

    private static void ConfigureIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction(ExtractsZipArchiveExtractDataSetBuilder builder, ExtractsZipArchiveChangeDataSetBuilderContext context)
    {
        var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();

        var intersection = roadSegment1Geometry.Centroid;

        var roadSegment2Geometry = new LineString(new Coordinate[]
        {
            new (intersection.X, intersection.Y - 1),
            new (intersection.X, intersection.Y + 5)
        });
        builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();

        builder.TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
        builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
        builder.TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;

        builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
    }

    [Fact]
    public async Task IntersectingOutlinedRoadSegmentsWithoutGradeSeparatedJunctionShouldNotGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder(fixture =>
            {
                fixture.CustomizeRoadSegmentOutlineLaneCount();
                fixture.CustomizeRoadSegmentOutlineMorphology();
                fixture.CustomizeRoadSegmentOutlineStatus();
                fixture.CustomizeRoadSegmentOutlineWidth();
            })
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction(builder, context);

                builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment2DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment2DbaseRecord.E_WK_OIDN.Value = 0;
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task IntersectingRoadSegmentsAtTheirStartOrEndPointsWithoutGradeSeparatedJunctionShouldNotGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();

                var intersection = roadSegment1Geometry.StartPoint;

                var roadSegment2Geometry = new LineString(new Coordinate[]
                {
                    new (intersection.X, intersection.Y),
                    new (intersection.X, intersection.Y + 5)
                });
                builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();

                builder.TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
                builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
                builder.TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;

                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task IntersectingRoadSegmentsInA_TShape_WithoutGradeSeparatedJunctionShouldNotGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();

                var intersection = roadSegment1Geometry.Centroid;

                var roadSegment2Geometry = new LineString(new Coordinate[]
                {
                    new (intersection.X, intersection.Y),
                    new (intersection.X, intersection.Y + 5)
                });
                builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();

                builder.TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
                builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
                builder.TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;

                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task Updated_DifferentId()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                var gradeSeparatedJunctionDbaseRecord2 = builder.CreateGradeSeparatedJunctionDbaseRecord();
                gradeSeparatedJunctionDbaseRecord2.BO_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value;
                gradeSeparatedJunctionDbaseRecord2.ON_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value;
                gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value = fixture.CreateWhichIsDifferentThan(new GradeSeparatedJunctionId(builder.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value));
                gradeSeparatedJunctionDbaseRecord2.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.DataSet.GradeSeparatedJunctionDbaseRecords = new[] { gradeSeparatedJunctionDbaseRecord2 }.ToList();
            })
            .BuildWithResult(context =>
            {
                var gradeSeparatedJunctionDbaseRecord2 = context.Change.DataSet.GradeSeparatedJunctionDbaseRecords.Single();

                return TranslatedChanges.Empty
                    .AppendChange(
                        new AddGradeSeparatedJunction
                        (
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value),
                            GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord2.TYPE.Value],
                            new RoadSegmentId(gradeSeparatedJunctionDbaseRecord2.BO_WS_OIDN.Value),
                            new RoadSegmentId(gradeSeparatedJunctionDbaseRecord2.ON_WS_OIDN.Value)
                        )
                    )
                    .AppendChange(
                        new RemoveGradeSeparatedJunction
                        (
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        )
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Updated_SameId()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new AddGradeSeparatedJunction
                        (
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            GradeSeparatedJunctionType.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value)
                        )
                    )
                    .AppendChange(
                        new RemoveGradeSeparatedJunction
                        (
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        )
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task RemovingDuplicateRecordsShouldReturnExpectedResult()
    {
        var zipArchiveBuilder = new ExtractsZipArchiveBuilder();

        var duplicateGradeSeparatedJunction = zipArchiveBuilder.Records.CreateGradeSeparatedJunctionDbaseRecord();

        var (zipArchive, expected) = zipArchiveBuilder
            .WithExtract((builder, context) =>
            {
                duplicateGradeSeparatedJunction.BO_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value;
                duplicateGradeSeparatedJunction.ON_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value;

                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Add(duplicateGradeSeparatedJunction);
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new RemoveGradeSeparatedJunction
                        (
                            new RecordNumber(2),
                            new GradeSeparatedJunctionId(duplicateGradeSeparatedJunction.OK_OIDN.Value)
                        )
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
