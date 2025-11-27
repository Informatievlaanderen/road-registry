namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using GradeSeparatedJunction.Changes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare.V3;
using RoadRegistry.BackOffice.FeatureCompare.V3.RoadSegment;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.BackOffice.FeatureCompare.V3.TranslatedChanges;

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

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.BEHEER.Value = orgId; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(organizationCache);
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));
        Assert.NotEmpty(ex.Problems);
        Assert.True(ex.Problems.All(x => x.Reason == "RoadSegmentMaintenanceAuthorityNotKnown"));
    }

    [Fact]
    public async Task SegmentWithTooLongGeometryShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create();
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));
        Assert.NotEmpty(ex.Problems);
        var problem = ex.Problems.First();
        Assert.Equal("RoadSegmentGeometryLengthTooLong", problem.Reason);
    }

    [Fact]
    public async Task WhenLeftSideStreetNameIdIsZero_ThenLeftStreetNameIdOutOfRange()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 0; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        ex.Problems.Should().ContainSingle(x => x.Reason == "LeftStreetNameIdOutOfRange");
    }

    [Fact]
    public async Task WhenLeftSideStreetNameIdIsNull_ThenNotApplicableIsUsedSilently()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 5;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 6;
            })
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = null; })
            .Build();

        var result = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Left).Value);
    }

    [Fact]
    public async Task WhenRightSideStreetNameIdIsZero_ThenRightStreetNameIdOutOfRange()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 0; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        ex.Problems.Should().ContainSingle(x => x.Reason == "RightStreetNameIdOutOfRange");
    }

    [Fact]
    public async Task WhenRightSideStreetNameIdIsNull_ThenNotApplicableIsUsedSilently()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 5;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 6;
            })
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = null; })
            .Build();

        var result = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Right).Value);
    }

    [Fact]
    public async Task SegmentWithRemovedLeftSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var removedStreetNameId = 1;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(removedStreetNameId, string.Empty, string.Empty, isRemoved: true);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.True((await streetNameCache.GetAsync(removedStreetNameId, CancellationToken.None)).IsRemoved);

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = removedStreetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Left || x.Side == RoadSegmentAttributeSide.Both).Value);
        //TODO-pr nog te bekijken of we nog die warnings gaan behouden of niet
        // Assert.NotEmpty(problems);
        // Assert.True(problems.All(x => x.Reason == "LeftStreetNameIdIsRemoved"));
    }

    [Fact]
    public async Task SegmentWithRemovedRightSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var removedStreetNameId = 1;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(removedStreetNameId, string.Empty, string.Empty, isRemoved: true);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.True((await streetNameCache.GetAsync(removedStreetNameId, CancellationToken.None)).IsRemoved);

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = removedStreetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Right || x.Side == RoadSegmentAttributeSide.Both).Value);
        //TODO-pr nog te bekijken of we nog die warnings gaan behouden of niet
        // Assert.NotEmpty(problems);
        // Assert.True(problems.All(x => x.Reason == "RightStreetNameIdIsRemoved"));
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

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = streetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(renamedToStreetNameId, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Left || x.Side == RoadSegmentAttributeSide.Both).Value);
        //TODO-pr nog te bekijken of we nog die warnings gaan behouden of niet
        // Assert.NotEmpty(problems);
        // Assert.True(problems.All(x => x.Reason == "LeftStreetNameIdIsRenamed"));
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

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = streetNameId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(renamedToStreetNameId, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Right || x.Side == RoadSegmentAttributeSide.Both).Value);
        //TODO-pr nog te bekijken of we nog die warnings gaan behouden of niet
        // Assert.NotEmpty(problems);
        // Assert.True(problems.All(x => x.Reason == "RightStreetNameIdIsRenamed"));
    }

    [Fact]
    public async Task WhenUnknownLeftStreetNameId_ThenLeftStreetNameIdOutOfRangeProblem()
    {
        var streetNameId = 1;

        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(new FakeStreetNameCache());

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = streetNameId; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var act = () => TranslateSucceeds(zipArchive, translator);
        var assert = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        assert.Where(ex => ex.Problems.Any(x => x.Reason == "LeftStreetNameIdOutOfRange"));
    }

    [Fact]
    public async Task WhenUnknownRightStreetNameId_ThenRightStreetNameIdOutOfRangeProblem()
    {
        var streetNameId = 1;

        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(new FakeStreetNameCache());

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = streetNameId; })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var act = () => TranslateSucceeds(zipArchive, translator);
        var assert = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        assert.Where(ex => ex.Problems.Any(x => x.Reason == "RightStreetNameIdOutOfRange"));
    }

    [Fact]
    public async Task ModifiedGeometrySlightly()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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
                .AppendChange(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry,
                    SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                        RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value])
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenGeometryHasAtLeast70PercentOverlap_ThenExtractIdShouldBeReused()
    {
        var archiveBuilder = new ExtractsZipArchiveBuilder();

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
                    new ModifyRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value),
                        OriginalId = newSegmentId,
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment2DbaseRecord.STATUS.Value]),
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                            RoadSegmentPosition.FromDouble(context.Change.TestData.RoadSegment2SurfaceDbaseRecord.VANPOS.Value!.Value),
                            RoadSegmentPosition.FromDouble(context.Change.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value!.Value),
                            RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment2SurfaceDbaseRecord.TYPE.Value]
                        )
                    }
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ModifiedGeometryToLessThan70PercentOverlap()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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
                    .AppendChange(new AddRoadSegmentChange
                    {
                        TemporaryId = roadSegment1Id,
                        OriginalId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        StartNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                        EndNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                        Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry,
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                        MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEER.Value!)),
                        Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORF.Value]),
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value]),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.WEGCAT.Value]),
                        AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value]),
                        StreetNameId = BuildStreetNameIdAttributes(StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value), StreetNameLocalId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)),
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                            RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value]
                        ),
                        EuropeanRoadNumbers = [
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!),
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)
                        ],
                        NationalRoadNumbers = [
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value!),
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value!)
                        ]
                    })
                    .AppendChange(new RemoveRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value)
                    })
                    .AppendChange(new RemoveRoadSegmentFromEuropeanRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                    })
                    .AppendChange(new RemoveRoadSegmentFromEuropeanRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                    })
                    .AppendChange(new RemoveRoadSegmentFromNationalRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                    })
                    .AppendChange(new RemoveRoadSegmentFromNationalRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                    })
                    .AppendChange(new AddGradeSeparatedJunctionChange
                    {
                        TemporaryId = new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                        Type = GradeSeparatedJunctionType.ByIdentifier[context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                        UpperRoadSegmentId = roadSegment1Id,
                        LowerRoadSegmentId = new RoadSegmentId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value)
                    })
                    .AppendChange(new RemoveGradeSeparatedJunctionChange
                    {
                        GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                    })
                    ;
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ModifiedNonCriticalAttribute()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.STATUS.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(new ModifyRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value])
                    });
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task AddingNewSegmentWith70OverlapToExistingShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
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
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = 0;
                builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = 0;

                // allowed for both outlined and measured
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = RoadSegmentMorphology.ServiceRoad.Translation.Identifier;

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
                .AppendChange(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(context.Change.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                    Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode1DbaseRecord.TYPE.Value],
                    Geometry = context.Change.TestData.RoadNode1ShapeRecord.Geometry
                })
                .AppendChange(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                    Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode2DbaseRecord.TYPE.Value],
                    Geometry = context.Change.TestData.RoadNode2ShapeRecord.Geometry
                })
                .AppendChange(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    StartNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                    EndNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value]
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task MultipleOutlinedRoadSegmentsWithIdenticalGeometriesShouldNotBeAProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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
                .AppendChange(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry,
                    SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                        RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value])
                })
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task MissingIntegrationProjectionFileShouldNotFail()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .ExcludeFileNames("IWEGSEGMENT.PRJ")
            .Build();

        var hasFile = zipArchive.Entries.Any(x => string.Equals(x.Name, "IWEGSEGMENT.PRJ", StringComparison.InvariantCultureIgnoreCase));
        Assert.False(hasFile);

        await TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty);
    }

    [Fact]
    public async Task IdsShouldBeUniqueAcrossChangeAndIntegrationData()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
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
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var extractCategory = RoadSegmentCategory.ByIdentifier[context.Extract.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value];
                builder.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value = context.Fixture.CreateWhichIsDifferentThan(extractCategory).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(new ModifyRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value])
                    });
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task GivenTwoIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentIsModified_ThenSecondIdIsUsed()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
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

        var translatedChanges = await TranslateSucceeds(zipArchive);

        var modifyRoadSegment2 = (ModifyRoadSegmentChange)translatedChanges.First();
        modifyRoadSegment2.OriginalId.Should().BeNull();
    }

    [Fact]
    public async Task GivenTwoIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentGeometryIsSlightlyChanged_ThenSecondIdIsUsed()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
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

        var translatedChanges = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment2 = (ModifyRoadSegmentChange)translatedChanges.First();
        modifyRoadSegment2.OriginalId.Should().BeNull();
    }

    [Fact]
    public async Task GivenTwoIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentIsUnchanged_ThenSecondIdIsUsed()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
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

                builder.TestData.RoadSegment2SurfaceDbaseRecord.TYPE.Value = context.Fixture.CreateWhichIsDifferentThan(
                    RoadSegmentSurfaceType.ByIdentifier[builder.TestData.RoadSegment2SurfaceDbaseRecord.TYPE.Value]
                ).Translation.Identifier;
            })
            .Build();

        var translatedChanges = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment2 = (ModifyRoadSegmentChange)translatedChanges.ToList()[1];
        modifyRoadSegment2.OriginalId.Should().Be(modifyRoadSegment2.RoadSegmentId);
    }

    [Fact]
    public async Task WhenOnlyIdChanges_ThenNoChangesDetected()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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
