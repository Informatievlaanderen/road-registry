namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios.Inwinning;

using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Xunit.Abstractions;

public partial class GradeSeparatedJunctionScenarios : FeatureCompareTranslatorScenariosBase
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
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
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
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
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
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
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
