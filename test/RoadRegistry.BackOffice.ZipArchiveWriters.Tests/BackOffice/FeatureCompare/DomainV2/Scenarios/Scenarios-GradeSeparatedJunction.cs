namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
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
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment1DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment1ShapeRecord }.ToList();
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange));
    }

    [Fact]
    public async Task WithNotUniqueIdentifier_ThenProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var duplicate = builder.CreateGradeSeparatedJunctionDbaseRecord();
                duplicate.OK_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value;
                duplicate.ON_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value;
                duplicate.BO_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value;
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Add(duplicate);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.IdentifierNotUnique));
    }

    [Fact]
    public async Task WithMultipleJunctionsWithSameLowerAndUpper_ThenProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var duplicate = builder.CreateGradeSeparatedJunctionDbaseRecord();
                duplicate.ON_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value;
                duplicate.BO_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value;
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Add(duplicate);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionNotUnique));
    }

    [Fact]
    public async Task EqualLowerAndUpperShouldGiveProblem()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value; })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment));
    }

    [Fact]
    public async Task UnknownRoadSegmentShouldGiveProblem()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value);

                builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value = fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value,
                    builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange));
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunction_ThenShouldGiveProblem()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange(ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunction)
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionMissing));
    }

    [Fact]
    public async Task WhenIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunction_WithChangedRoadSegmentsWithNullGeometry_ThenShouldOnlyReturnProblem()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                // trigger partial update
                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentMorphologyV2.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.MORF.Value]).Translation.Identifier;

                // add segment which intersects with existing segment to trigger checking missing gradeseparatedjunctions
                var newNode1Dbase = builder.CreateRoadNodeDbaseRecord();
                var newNode1Shape = builder.CreateRoadNodeShapeRecord();
                var newNode2Dbase = builder.CreateRoadNodeDbaseRecord();
                var newNode2Shape = builder.CreateRoadNodeShapeRecord();
                var newSegment1Dbase = builder.CreateRoadSegmentDbaseRecord();
                newSegment1Dbase.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd;
                var newSegment1Shape = builder.CreateRoadSegmentShapeRecord();
                var intersection = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString().Centroid;
                var newSegment1GeometryCrossingSegment1 = new LineString([
                    new(intersection.X, intersection.Y - 1),
                    new(intersection.X, intersection.Y + 5)
                ]);
                newSegment1Shape.Geometry = newSegment1GeometryCrossingSegment1.ToMultiLineString();

                builder.DataSet.RoadNodeDbaseRecords.Add(newNode1Dbase);
                builder.DataSet.RoadNodeDbaseRecords.Add(newNode2Dbase);
                builder.DataSet.RoadNodeShapeRecords.Add(newNode1Shape);
                builder.DataSet.RoadNodeShapeRecords.Add(newNode2Shape);
                builder.DataSet.RoadSegmentDbaseRecords.Add(newSegment1Dbase);
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1Shape);

                foreach (var roadSegmentDbaseRecord in builder.DataSet.RoadSegmentDbaseRecords)
                {
                    roadSegmentDbaseRecord.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd;
                }
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionMissing));
    }

    private static void ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunction(ExtractsZipArchiveExtractDataSetBuilder builder, ExtractsZipArchiveChangeDataSetBuilderContext context)
    {
        foreach (var roadSegmentDbaseRecord in builder.DataSet.RoadSegmentDbaseRecords)
        {
            roadSegmentDbaseRecord.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd;
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
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture =>
            {
                fixture.CustomizeRoadSegmentOutlineLaneCount();
                fixture.CustomizeRoadSegmentOutlineMorphology();
                fixture.CustomizeRoadSegmentOutlineStatus();
                fixture.CustomizeRoadSegmentOutlineWidth();
            })
            .WithChange((builder, context) =>
            {
                ConfigureIntersectingGerealiseerdRoadSegmentsWithoutGradeSeparatedJunction(builder, context);

                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentStatusV2.Gerealiseerd);
                builder.TestData.RoadSegment2DbaseRecord.STATUS.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentStatusV2.Gerealiseerd);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task IntersectingRoadSegmentsInA_TShape_WithoutGradeSeparatedJunctionShouldNotGiveProblem()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture =>
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
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateSucceeds(zipArchive);
    }

    [Fact]
    public async Task WhenChangingSegments_ThenNewId()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                var gradeSeparatedJunctionDbaseRecord2 = builder.CreateGradeSeparatedJunctionDbaseRecord();
                gradeSeparatedJunctionDbaseRecord2.BO_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value;
                gradeSeparatedJunctionDbaseRecord2.ON_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value;
                gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value = fixture.CreateWhichIsDifferentThan(new GradeSeparatedJunctionId(builder.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value));
                gradeSeparatedJunctionDbaseRecord2.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionTypeV2.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.DataSet.GradeSeparatedJunctionDbaseRecords = new[] { gradeSeparatedJunctionDbaseRecord2 }.ToList();
            })
            .BuildWithResult(context =>
            {
                var gradeSeparatedJunctionDbaseRecord2 = context.Change.DataSet.GradeSeparatedJunctionDbaseRecords.Single();

                return TranslatedChanges.Empty
                    .AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value),
                            Type = GradeSeparatedJunctionTypeV2.ByIdentifier[gradeSeparatedJunctionDbaseRecord2.TYPE.Value],
                            UpperRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value)
                        }
                    )
                    .AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenChangingType_ThenIdIsKept()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionTypeV2.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            Type = GradeSeparatedJunctionTypeV2.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            UpperRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value)
                        }
                    )
                    .AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task RemovingDuplicateRecordsShouldReturnExpectedResult()
    {
        var zipArchiveBuilder = new DomainV2ZipArchiveBuilder();

        var duplicateGradeSeparatedJunction = zipArchiveBuilder.Records.CreateGradeSeparatedJunctionDbaseRecord();

        var (zipArchive, expected) = zipArchiveBuilder
            .WithExtract((builder, context) =>
            {
                duplicateGradeSeparatedJunction.BO_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value;
                duplicateGradeSeparatedJunction.ON_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value;

                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Add(duplicateGradeSeparatedJunction);
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(duplicateGradeSeparatedJunction.OK_OIDN.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
