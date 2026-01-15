namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V2.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare.V2;
using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V1;
using Xunit.Abstractions;
using RoadSegmentLaneAttribute = Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = Uploads.RoadSegmentWidthAttribute;

public class RoadSegmentScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadSegmentScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task SegmentWithUnknownMaintenanceAuthorityShouldGiveProblem()
    {
        var orgId = new OrganizationId("OVO999999");
        var organizationCache = new FakeOrganizationCache()
            .Seed(orgId, null);

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.BEHEER.Value = orgId; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(organizationCache);
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));
        Assert.NotEmpty(ex.Problems);
        Assert.True(ex.Problems.All(x => x.Reason == "RoadSegmentMaintenanceAuthorityNotKnown"));
    }

    [Fact]
    public async Task SegmentWithTooLongGeometryShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString([
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 100000, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 100000)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create();
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));
        Assert.NotEmpty(ex.Problems);
        var problem = ex.Problems.First();
        Assert.Equal("RoadSegmentGeometryLengthTooLong", problem.Reason);
    }

    [Fact]
    public async Task WhenLeftSideStreetNameIdIsZero_ThenLeftStreetNameIdOutOfRange()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 0; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        ex.Problems.Should().ContainSingle(x => x.Reason == "LeftStreetNameIdOutOfRange");
    }

    [Fact]
    public async Task WhenLeftSideStreetNameIdIsNull_ThenNotApplicableIsUsedSilently()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 6; })
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = null; })
            .Build();

        var (result, _) = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment = Assert.IsType<ModifyRoadSegment>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.LeftSideStreetNameId);
    }

    [Fact]
    public async Task WhenRightSideStreetNameIdIsZero_ThenRightStreetNameIdOutOfRange()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 0; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        ex.Problems.Should().ContainSingle(x => x.Reason == "RightStreetNameIdOutOfRange");
    }

    [Fact]
    public async Task WhenRightSideStreetNameIdIsNull_ThenNotApplicableIsUsedSilently()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = null; })
            .Build();

        var (result, _) = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment = Assert.IsType<ModifyRoadSegment>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.RightSideStreetNameId);
    }

    [Fact]
    public async Task SegmentWithRemovedLeftSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var removedStreetNameId = 1;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(removedStreetNameId, string.Empty, string.Empty, isRemoved: true);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.True((await streetNameCache.GetAsync(removedStreetNameId, CancellationToken.None)).IsRemoved);

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = removedStreetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var (result, problems) = await TranslateSucceeds(zipArchive, translator, validator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegment>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.LeftSideStreetNameId);

        Assert.NotEmpty(problems);
        Assert.True(problems.All(x => x.Reason == "LeftStreetNameIdIsRemoved"));
    }

    [Fact]
    public async Task SegmentWithRemovedRightSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var removedStreetNameId = 1;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(removedStreetNameId, string.Empty, string.Empty, isRemoved: true);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.True((await streetNameCache.GetAsync(removedStreetNameId, CancellationToken.None)).IsRemoved);

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = removedStreetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var (result, problems) = await TranslateSucceeds(zipArchive, translator, validator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegment>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.RightSideStreetNameId);

        Assert.NotEmpty(problems);
        Assert.True(problems.All(x => x.Reason == "RightStreetNameIdIsRemoved"));
    }

    [Fact]
    public async Task SegmentWithRenamedLeftSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var streetNameId = 1;
        var renamedToStreetNameId = 2;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(streetNameId, string.Empty, string.Empty)
            .AddRenamedStreetName(streetNameId, renamedToStreetNameId);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.Equal(renamedToStreetNameId, (await streetNameCache.GetRenamedIdsAsync(new[] { streetNameId }, CancellationToken.None))[streetNameId]);

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = streetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var (result, problems) = await TranslateSucceeds(zipArchive, translator, validator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegment>(Assert.Single(result));
        Assert.Equal(renamedToStreetNameId, modifyRoadSegment.LeftSideStreetNameId!.Value.ToInt32());

        Assert.NotEmpty(problems);
        Assert.True(problems.All(x => x.Reason == "LeftStreetNameIdIsRenamed"));
    }

    [Fact]
    public async Task SegmentWithRenamedRightSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var streetNameId = 1;
        var renamedToStreetNameId = 2;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(streetNameId, string.Empty, string.Empty)
            .AddRenamedStreetName(streetNameId, renamedToStreetNameId);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.Equal(renamedToStreetNameId, (await streetNameCache.GetRenamedIdsAsync(new[] { streetNameId }, CancellationToken.None))[streetNameId]);

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = streetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var (result, problems) = await TranslateSucceeds(zipArchive, translator, validator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegment>(Assert.Single(result));
        Assert.Equal(renamedToStreetNameId, modifyRoadSegment.RightSideStreetNameId!.Value.ToInt32());

        Assert.NotEmpty(problems);
        Assert.True(problems.All(x => x.Reason == "RightStreetNameIdIsRenamed"));
    }

    [Fact]
    public async Task WhenUnknownLeftStreetNameId_ThenLeftStreetNameIdOutOfRangeProblem()
    {
        var streetNameId = 1;

        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(new FakeStreetNameCache());

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = streetNameId; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var act = () => TranslateSucceeds(zipArchive, translator, validator);
        var assert = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        assert.Where(ex => ex.Problems.Any(x => x.Reason == "LeftStreetNameIdOutOfRange"));
    }

    [Fact]
    public async Task WhenUnknownRightStreetNameId_ThenRightStreetNameIdOutOfRangeProblem()
    {
        var streetNameId = 1;

        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(new FakeStreetNameCache());

        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = streetNameId; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var act = () => TranslateSucceeds(zipArchive, translator, validator);
        var assert = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        assert.Where(ex => ex.Problems.Any(x => x.Reason == "RightStreetNameIdOutOfRange"));
    }

    [Fact]
    public async Task ModifiedGeometrySlightly()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 1)
                });
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new ModifyRoadSegment(
                        new RecordNumber(1),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                        geometry: context.Change.TestData.RoadSegment1ShapeRecord.Geometry
                    )
                    .WithLane(
                        new RoadSegmentLaneAttribute(
                            new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                            new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                            RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                        )
                    )
                    .WithWidth(
                        new RoadSegmentWidthAttribute(
                            new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                            new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                        )
                    )
                    .WithSurface(
                        new RoadSegmentSurfaceAttribute(
                            new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                            RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                        )
                    )));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ExtractIdShouldBeReusedWhenGeometryHas70Overlap()
    {
        var archiveBuilder = new ExtractV1ZipArchiveBuilder();

        var newSegmentId = new RoadSegmentId(0);

        var (zipArchive, expected) = archiveBuilder
            .WithChange((builder, context) =>
            {
                newSegmentId = archiveBuilder.Fixture.CreateWhichIsDifferentThan(
                    new RoadSegmentId(builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    new RoadSegmentId(builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value));

                builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment2DbaseRecord.STATUS.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[builder.TestData.RoadSegment2DbaseRecord.STATUS.Value]).Translation.Identifier;

                builder.TestData.RoadSegment2LaneDbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment2SurfaceDbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment2WidthDbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = newSegmentId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new ModifyRoadSegment(
                            new RecordNumber(2),
                            new RoadSegmentId(context.Extract.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment2DbaseRecord.METHODE.Value],
                            status: RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment2DbaseRecord.STATUS.Value],
                            originalId: newSegmentId
                        )
                        .WithLane(
                            new RoadSegmentLaneAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment2LaneDbaseRecord.RS_OIDN.Value),
                                new RoadSegmentLaneCount(context.Change.TestData.RoadSegment2LaneDbaseRecord.AANTAL.Value),
                                RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment2LaneDbaseRecord.RICHTING.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment2LaneDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value))
                            )
                        )
                        .WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment2WidthDbaseRecord.WB_OIDN.Value),
                                new RoadSegmentWidth(context.Change.TestData.RoadSegment2WidthDbaseRecord.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment2WidthDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value))
                            )
                        )
                        .WithSurface(
                            new RoadSegmentSurfaceAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment2SurfaceDbaseRecord.WV_OIDN.Value),
                                RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment2SurfaceDbaseRecord.TYPE.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment2SurfaceDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value))
                            )
                        )
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ModifiedGeometryToLessThan70PercentOverlap()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                var lineString = new LineString(new Coordinate[]
                {
                    new CoordinateM(100000, 100000, 0),
                    new CoordinateM(100100, 100000, 0)
                });
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
            })
            .WithChange((builder, context) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 9000, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 9000)
                });
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
            })
            .BuildWithResult(context =>
            {
                var maxRoadSegmentId = context.GetMaxRoadSegmentId();
                var roadSegment1Id = maxRoadSegmentId.Next();

                return TranslatedChanges.Empty
                        .AppendChange(new AddRoadSegment(
                                new RecordNumber(1),
                                roadSegment1Id,
                                new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                                new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                                new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                                new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                                RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORF.Value],
                                RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                                RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.WEGCAT.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                                StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                                StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
                            )
                            .WithGeometry(context.Change.TestData.RoadSegment1ShapeRecord.Geometry)
                            .WithLane(
                                new RoadSegmentLaneAttribute(
                                    new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                                    new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                                    RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                                )
                            )
                            .WithWidth(
                                new RoadSegmentWidthAttribute(
                                    new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                                    new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                                )
                            )
                            .WithSurface(
                                new RoadSegmentSurfaceAttribute(
                                    new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                                    RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                                    new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                                )
                            ))
                        .AppendChange(new RemoveRoadSegment(
                            new RecordNumber(1),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value]
                        ))
                        .AppendChange(new AddRoadSegmentToEuropeanRoad(
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            roadSegment1Id,
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                        ))
                        .AppendChange(new AddRoadSegmentToEuropeanRoad(
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            roadSegment1Id,
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                        ))
                        .AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                        ))
                        .AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                        ))
                        .AppendChange(new AddRoadSegmentToNationalRoad(
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            roadSegment1Id,
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                        ))
                        .AppendChange(new AddRoadSegmentToNationalRoad(
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            roadSegment1Id,
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                        ))
                        .AppendChange(new RemoveRoadSegmentFromNationalRoad(
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                        ))
                        .AppendChange(new RemoveRoadSegmentFromNationalRoad(
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                        ))
                        .AppendChange(new AddRoadSegmentToNumberedRoad(
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            roadSegment1Id,
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value),
                            RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.RICHTING.Value],
                            new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.VOLGNUMMER.Value)
                        ))
                        .AppendChange(new AddRoadSegmentToNumberedRoad(
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.GW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            roadSegment1Id,
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value),
                            RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.RICHTING.Value],
                            new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.VOLGNUMMER.Value)
                        ))
                        .AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value)
                        ))
                        .AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.GW_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value)
                        ))
                        .AppendChange(new AddGradeSeparatedJunction(
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            GradeSeparatedJunctionType.ByIdentifier[context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            roadSegment1Id,
                            new RoadSegmentId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value)
                        ))
                        .AppendChange(new RemoveGradeSeparatedJunction(
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        ))
                    ;
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ModifiedNonCriticalAttribute()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.STATUS.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(new ModifyRoadSegment(
                        new RecordNumber(1),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                        status: RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value]
                    ));
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task AddingNewSegmentWith70OverlapToExistingShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var roadNodeDbaseRecord1 = builder.CreateRoadNodeDbaseRecord();
                var roadNodeDbaseRecord2 = builder.CreateRoadNodeDbaseRecord();
                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord());
                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord());
                builder.DataSet.RoadNodeDbaseRecords.Add(roadNodeDbaseRecord1);
                builder.DataSet.RoadNodeDbaseRecords.Add(roadNodeDbaseRecord2);

                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString([
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 0.1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 0.1)
                ]);
                var roadSegmentShapeRecord = builder.CreateRoadSegmentShapeRecord(lineString);
                builder.DataSet.RoadSegmentShapeRecords.Add(roadSegmentShapeRecord);

                var roadSegmentDbaseRecord = builder.CreateRoadSegmentDbaseRecord();
                roadSegmentDbaseRecord.B_WK_OIDN.Value = roadNodeDbaseRecord1.WK_OIDN.Value;
                roadSegmentDbaseRecord.E_WK_OIDN.Value = roadNodeDbaseRecord2.WK_OIDN.Value;
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord);

                var laneDbaseRecord = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord.WS_OIDN.Value;
                laneDbaseRecord.VANPOS.Value = 0;
                laneDbaseRecord.TOTPOS.Value = roadSegmentShapeRecord.Geometry.Length;
                builder.DataSet.LaneDbaseRecords.Add(laneDbaseRecord);

                var surfaceDbaseRecord = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord.VANPOS.Value = 0;
                surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeRecord.Geometry.Length;
                builder.DataSet.SurfaceDbaseRecords.Add(surfaceDbaseRecord);

                var widthDbaseRecord = builder.CreateRoadSegmentWidthDbaseRecord();
                widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord.WS_OIDN.Value;
                widthDbaseRecord.VANPOS.Value = 0;
                widthDbaseRecord.TOTPOS.Value = roadSegmentShapeRecord.Geometry.Length;
                builder.DataSet.WidthDbaseRecords.Add(widthDbaseRecord);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadSegmentIsAlreadyProcessed), problem.Reason);
    }

    [Fact]
    public async Task ConversionFromOutlinedToMeasuredShouldReturnExpectedResult()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = RoadSegmentMorphology.Ferry.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = 0;

                builder.DataSet.RoadNodeDbaseRecords.Remove(builder.TestData.RoadNode1DbaseRecord);
                builder.DataSet.RoadNodeDbaseRecords.Remove(builder.TestData.RoadNode2DbaseRecord);
                builder.DataSet.RoadNodeShapeRecords.Remove(builder.TestData.RoadNode1ShapeRecord);
                builder.DataSet.RoadNodeShapeRecords.Remove(builder.TestData.RoadNode2ShapeRecord);
            })
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Measured.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = builder.TestData.RoadNode2DbaseRecord.WK_OIDN.Value;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new AddRoadNode(
                        new RecordNumber(1),
                        new RoadNodeId(context.Change.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                        new RoadNodeId(context.Change.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                        RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode1DbaseRecord.TYPE.Value]
                    ).WithGeometry(context.Change.TestData.RoadNode1ShapeRecord.Geometry)
                )
                .AppendChange(
                    new AddRoadNode(
                        new RecordNumber(2),
                        new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                        new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                        RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode2DbaseRecord.TYPE.Value]
                    ).WithGeometry(context.Change.TestData.RoadNode2ShapeRecord.Geometry)
                )
                .AppendChange(
                    new ModifyRoadSegment(
                            new RecordNumber(1),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                            new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                            new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value),
                            RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value],
                            RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                            RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value],
                            RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                            StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                            StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value),
                            geometry: context.Change.TestData.RoadSegment1ShapeRecord.Geometry
                        )
                        .WithLane(
                            new RoadSegmentLaneAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                                new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                                RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                            )
                        )
                        .WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                                new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                            )
                        )
                        .WithSurface(
                            new RoadSegmentSurfaceAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                                RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                            )
                        )
                )
                .AppendChange(
                    new RemoveOutlinedRoadSegment
                    (
                        new RecordNumber(1),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value)
                    )
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task MultipleOutlinedRoadSegmentsWithIdenticalGeometriesShouldNotBeAProblem()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = RoadSegmentMorphology.Ferry.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = 0;

                builder.TestData.RoadSegment2DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.MORF.Value = RoadSegmentMorphology.Ferry.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment2DbaseRecord.E_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment2ShapeRecord.Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry;

                builder.DataSet.Clear();

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment1DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment1ShapeRecord);
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment1LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment1SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment1WidthDbaseRecord);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment2LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment2SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment2WidthDbaseRecord);
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.Clear();

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment1DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment1ShapeRecord);
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment1LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment1SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment1WidthDbaseRecord);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment2LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment2SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment2WidthDbaseRecord);
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task MultipleOutlinedRoadSegmentsWithOverlappingGeometriesButChangedGeometriesShouldReturnExpectedResult()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = RoadSegmentMorphology.Ferry.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = 0;

                builder.TestData.RoadSegment2DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.MORF.Value = RoadSegmentMorphology.Ferry.Translation.Identifier;
                builder.TestData.RoadSegment2DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment2DbaseRecord.E_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment2ShapeRecord.Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry;

                builder.DataSet.Clear();
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment1DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment1ShapeRecord);
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment1LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment1SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment1WidthDbaseRecord);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
                builder.TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment2ShapeRecord.Geometry.Length;
                builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment2ShapeRecord.Geometry.Length;
                builder.TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment2ShapeRecord.Geometry.Length;
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment2LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment2SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment2WidthDbaseRecord);
            })
            .WithChange((builder, context) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString([
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 1)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();

                builder.DataSet.Clear();
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment1DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment1ShapeRecord);

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment1LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment1SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment1WidthDbaseRecord);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
                builder.DataSet.LaneDbaseRecords.Add(builder.TestData.RoadSegment2LaneDbaseRecord);
                builder.DataSet.SurfaceDbaseRecords.Add(builder.TestData.RoadSegment2SurfaceDbaseRecord);
                builder.DataSet.WidthDbaseRecords.Add(builder.TestData.RoadSegment2WidthDbaseRecord);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new ModifyRoadSegment(
                            new RecordNumber(1),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            geometry: context.Change.TestData.RoadSegment1ShapeRecord.Geometry
                        )
                        .WithLane(
                            new RoadSegmentLaneAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                                new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                                RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                            )
                        )
                        .WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                                new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                            )
                        )
                        .WithSurface(
                            new RoadSegmentSurfaceAttribute(
                                new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                                RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                            )
                        )
                )
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task MissingIntegrationProjectionFileShouldNotFail()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .ExcludeFileNames("IWEGSEGMENT.PRJ")
            .Build();

        var hasFile = zipArchive.Entries.Any(x => string.Equals(x.Name, "IWEGSEGMENT.PRJ", StringComparison.InvariantCultureIgnoreCase));
        Assert.False(hasFile);

        await TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty);
    }

    [Fact]
    public async Task IdsShouldBeUniqueAcrossChangeAndIntegrationData()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var integrationRoadSegment = context.Integration.DataSet.RoadSegmentDbaseRecords.First();

                builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value = integrationRoadSegment.WS_OIDN.Value;
                builder.TestData.RoadSegment1LaneDbaseRecord.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                builder.TestData.RoadSegment1WidthDbaseRecord.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange), problem.Reason);
    }

    [Fact]
    public async Task WhenUserChangedCategory_ThenOnlyCategoryShouldBeNotNull()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var extractCategory = RoadSegmentCategory.ByIdentifier[context.Extract.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value];
                builder.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value = context.Fixture.CreateWhichIsDifferentThan(extractCategory).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new ModifyRoadSegment(
                            new RecordNumber(1),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            category: RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value]
                        )
                        //.WithGeometry(context.Change.TestData.RoadSegment1ShapeRecord.Geometry)
                        // .WithLane(
                        //     new RoadSegmentLaneAttribute(
                        //         new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                        //         new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                        //         RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                        //         new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                        //         new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                        //     )
                        // )
                        // .WithWidth(
                        //     new RoadSegmentWidthAttribute(
                        //         new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                        //         new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                        //         new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                        //         new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                        //     )
                        // )
                        // .WithSurface(
                        //     new RoadSegmentSurfaceAttribute(
                        //         new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                        //         RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                        //         new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        //         new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                        //     )
                        // )
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task GivenTwoIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentIsModified_ThenSecondIdIsUsed()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                // ensure geometries of roadsegment 1 and 2 are equal
                var geometry = builder.TestData.RoadSegment2ShapeRecord.Geometry;
                builder.TestData.RoadSegment1ShapeRecord.Geometry = geometry;
                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = geometry.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = geometry.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = geometry.Length;
            })
            .WithChange((builder, context) =>
            {
                var roadSegment1Id = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                builder.DataSet.RemoveRoadSegment(roadSegment1Id);

                // change roadsegment 2
                builder.TestData.RoadSegment2DbaseRecord.CATEGORIE.Value = context.Fixture.CreateWhichIsDifferentThan(
                    RoadSegmentCategory.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value],
                    RoadSegmentCategory.ByIdentifier[builder.TestData.RoadSegment2DbaseRecord.CATEGORIE.Value]
                ).Translation.Identifier;
            })
            .Build();

        var (translatedChanges, problems) = await TranslateSucceeds(zipArchive);
        problems.HasError().Should().BeFalse();
        var modifyRoadSegment2 = (ModifyRoadSegment)translatedChanges.First();
        modifyRoadSegment2.OriginalId.Should().BeNull();
    }

    [Fact]
    public async Task GivenTwoIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentGeometryIsSlightlyChanged_ThenSecondIdIsUsed()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                // ensure geometries of roadsegment 1 and 2 are equal
                var geometry = builder.TestData.RoadSegment2ShapeRecord.Geometry;
                builder.TestData.RoadSegment1ShapeRecord.Geometry = geometry;
                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = geometry.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = geometry.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = geometry.Length;
            })
            .WithChange((builder, context) =>
            {
                var roadSegment1Id = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                builder.DataSet.RemoveRoadSegment(roadSegment1Id);

                var lineString = builder.TestData.RoadSegment2ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 1)
                });
                builder.TestData.RoadSegment2ShapeRecord.Geometry = lineString.ToMultiLineString();
                builder.TestData.RoadSegment2LaneDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment2WidthDbaseRecord.TOTPOS.Value = lineString.Length;
            })
            .Build();

        var (translatedChanges, problems) = await TranslateSucceeds(zipArchive);
        problems.HasError().Should().BeFalse();
        var modifyRoadSegment2 = (ModifyRoadSegment)translatedChanges.First();
        modifyRoadSegment2.OriginalId.Should().BeNull();
    }

    [Fact]
    public async Task GivenTwoIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentIsUnchanged_ThenSecondIdIsUsed()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                // ensure geometries of roadsegment 1 and 2 are equal
                var geometry = builder.TestData.RoadSegment2ShapeRecord.Geometry;
                builder.TestData.RoadSegment1ShapeRecord.Geometry = geometry;
                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = geometry.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = geometry.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = geometry.Length;
            })
            .WithChange((builder, context) =>
            {
                var roadSegment1Id = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                builder.DataSet.RemoveRoadSegment(roadSegment1Id);

                builder.TestData.RoadSegment2LaneDbaseRecord.AANTAL.Value = context.Fixture.CreateWhichIsDifferentThan(
                    new RoadSegmentLaneCount(builder.TestData.RoadSegment2LaneDbaseRecord.AANTAL.Value)
                );
            })
            .Build();

        var (translatedChanges, problems) = await TranslateSucceeds(zipArchive);
        problems.HasError().Should().BeFalse();
        var modifyRoadSegment2 = (ModifyRoadSegment)translatedChanges.ToList()[1];
        modifyRoadSegment2.OriginalId.Should().BeNull();
    }

    [Fact]
    public async Task WhenOnlyIdChanges_ThenNoChangesDetected()
    {
        var (zipArchive, expected) = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var newSegmentId = builder.DataSet.RoadSegmentDbaseRecords.Select(x => x.WS_OIDN.Value).Max() + 1;
                builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1LaneDbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1WidthDbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1NationalRoadDbaseRecord1.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1NationalRoadDbaseRecord2.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord2.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.WS_OIDN.Value = newSegmentId;
                builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = newSegmentId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        await TranslateSucceeds(zipArchive);
    }

    private static void FillStreetNameCache(ExtractsZipArchiveExtractDataSetBuilder builder, FakeStreetNameCache streetNameCache)
    {
        var streetNameIds = builder.DataSet.RoadSegmentDbaseRecords
            .SelectMany(x => new[] { x.LSTRNMID.Value, x.RSTRNMID.Value })
            .Where(x => x is not null)
            .Select(x => x.Value)
            .Distinct()
            .ToList();

        foreach (var streetNameId in streetNameIds)
        {
            streetNameCache.AddStreetName(streetNameId, string.Empty, string.Empty);
        }
    }
}
