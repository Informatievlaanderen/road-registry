namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios.Inwinning;

using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadRegistry.Editor.Schema.Extensions;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Xunit.Abstractions;

public class RoadSegmentScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadSegmentScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task WhenEmptyDbfOrShp_ThenError()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                builder.DataSet.RoadSegmentDbaseRecords.Clear();
                builder.DataSet.RoadSegmentShapeRecords.Clear();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().Contain(x => x.File == "WEGSEGMENT.DBF" && x.Reason == nameof(DbaseFileProblems.HasNoDbaseRecords));
        ex.Problems.Should().Contain(x => x.File == "WEGSEGMENT.SHP" && x.Reason == nameof(ShapeFileProblems.HasNoShapeRecords));
    }

    [Fact]
    public async Task WhenGeometryIsOutsideTransactionZone_ThenProblem()
    {
        // Arrange
        var (zipArchive, _) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                var transactionZoneBoundaryCoordinates = builder.TestData.TransactionZoneShapeRecord.Geometry.Boundary.Coordinates;
                var pointOutsideTransactionZone = new Coordinate(transactionZoneBoundaryCoordinates.Max(x => x.X + 1), transactionZoneBoundaryCoordinates.Max(x => x.Y));
                builder.TestData.RoadSegment1ShapeRecord.Geometry = new LineString([pointOutsideTransactionZone, new Coordinate(pointOutsideTransactionZone.X + 100, pointOutsideTransactionZone.Y)])
                    .WithSrid(builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry.SRID)
                    .ToMultiLineString();
            })
            .BuildWithContext();

        // Act
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive));

        // Assert
        ex.Problems.Should().Contain(x => x.File == "WEGSEGMENT.SHP" && x.Reason == "ShapeRecordGeometryIsOutsideTransactionZone");
    }

    [Fact]
    public async Task SegmentWithUnknownMaintenanceAuthorityShouldGiveProblem()
    {
        var orgId = new OrganizationId("OVO999999");
        var organizationCache = new FakeOrganizationCache()
            .Seed(orgId, null);

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.LBEHEER.Value = orgId; })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(organizationCache);
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));

        Assert.NotEmpty(ex.Problems);
        Assert.True(ex.Problems.All(x => x.Reason == "RoadSegmentMaintenanceAuthorityNotKnown"));
    }

    [Fact]
    public async Task SegmentWithTooLongGeometryShouldGiveProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString([
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 100000, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 100000)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
            })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create();
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));

        Assert.NotEmpty(ex.Problems);
        var problem = ex.Problems.First();
        Assert.Equal("RoadSegmentGeometryLengthTooLong", problem.Reason);
    }

    [Fact]
    public async Task WhenLeftSideStreetNameIdIsZero_ThenLeftStreetNameIdOutOfRange()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 0; })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        ex.Problems.Should().ContainSingle(x => x.Reason == "LeftStreetNameIdOutOfRange");
    }

    [Fact]
    public async Task WhenLeftSideStreetNameIdIsNull_ThenNotApplicableIsUsedSilently()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 5;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 6;
            })
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = null; })
            .Build();

        var result = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side == RoadSegmentAttributeSide.Left).Value);
    }

    [Fact]
    public async Task WhenRightSideStreetNameIdIsZero_ThenRightStreetNameIdOutOfRange()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 0; })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        ex.Problems.Should().ContainSingle(x => x.Reason == "RightStreetNameIdOutOfRange");
    }

    [Fact]
    public async Task WhenRightSideStreetNameIdIsNull_ThenNotApplicableIsUsedSilently()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = 5;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = 6;
            })
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = null; })
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

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = removedStreetNameId;
            })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side is RoadSegmentAttributeSide.Left or RoadSegmentAttributeSide.Both).Value);
    }

    [Fact]
    public async Task SegmentWithRemovedRightSideStreetNameShouldGiveWarningAndFixAutomatically()
    {
        var removedStreetNameId = 1;

        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(removedStreetNameId, string.Empty, string.Empty, isRemoved: true);
        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(streetNameCache);

        Assert.True((await streetNameCache.GetAsync(removedStreetNameId, CancellationToken.None)).IsRemoved);

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = removedStreetNameId;
            })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(StreetNameLocalId.NotApplicable, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side is RoadSegmentAttributeSide.Right or RoadSegmentAttributeSide.Both).Value);
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

        Assert.Equal(renamedToStreetNameId, (await streetNameCache.GetRenamedIdsAsync([streetNameId], CancellationToken.None))[streetNameId]);

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = streetNameId;
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = StreetNameLocalId.NotApplicable;
            })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(renamedToStreetNameId, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side is RoadSegmentAttributeSide.Left or RoadSegmentAttributeSide.Both).Value);
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

        Assert.Equal(renamedToStreetNameId, (await streetNameCache.GetRenamedIdsAsync([streetNameId], CancellationToken.None))[streetNameId]);

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                FillStreetNameCache(builder, streetNameCache);
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = streetNameId;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = StreetNameLocalId.NotApplicable;
            })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var result = await TranslateSucceeds(zipArchive, translator);

        var modifyRoadSegment = Assert.IsType<ModifyRoadSegmentChange>(Assert.Single(result));
        Assert.Equal(renamedToStreetNameId, modifyRoadSegment.StreetNameId!.Values.Single(x => x.Side is RoadSegmentAttributeSide.Right or RoadSegmentAttributeSide.Both).Value);
    }

    [Fact]
    public async Task WhenUnknownLeftStreetNameId_ThenLeftStreetNameIdOutOfRangeProblem()
    {
        var streetNameId = 1;

        var streetNameContextFactory = new RoadSegmentFeatureCompareStreetNameContextFactory(new FakeStreetNameCache());

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = streetNameId; })
            .Build();

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

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) => { builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = streetNameId; })
            .Build();

        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(streetNameContextFactory: streetNameContextFactory);
        var act = () => TranslateSucceeds(zipArchive, translator);

        var assert = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        assert.Where(ex => ex.Problems.Any(x => x.Reason == "RightStreetNameIdOutOfRange"));
    }

    [Fact]
    public async Task WhenModifiedGeometrySlightly_ThenRoadSegmentModified()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = lineString.Factory.CreateLineString([
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 1)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(BuildModifyRoadSegmentChange(context.Change.TestData.RoadSegment1DbaseRecord, context.Change.TestData.RoadSegment1ShapeRecord) with
                {
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
                })
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenGeometryHasAtLeast70PercentOverlap_ThenExtractIdShouldBeReusedAndJunctionUnchanged()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, context) =>
            {
                var newSegmentTempId = context.Fixture.CreateWhichIsDifferentThan(
                    new RoadSegmentTempId(builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value),
                    new RoadSegmentTempId(builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value));
                var newSegmentId = context.Fixture.CreateWhichIsDifferentThan(
                    new RoadSegmentId(builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                    new RoadSegmentId(builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value));

                builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value = newSegmentTempId.ToInt32();
                builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value = newSegmentId;
                builder.TestData.RoadSegment2DbaseRecord.MORF.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentMorphologyV2.ByIdentifier[builder.TestData.RoadSegment2DbaseRecord.MORF.Value]);

                builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value = newSegmentTempId.ToInt32();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(BuildModifyRoadSegmentChange(context.Change.TestData.RoadSegment2DbaseRecord, context.Change.TestData.RoadSegment2ShapeRecord) with
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(context.Extract.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value),
                        [new RoadSegmentTempId(context.Change.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value)]),
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenModifiedGeometryToLessThan70PercentOverlap_ThenNewSegmentAdded()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.Factory.CreateLineString([
                    new Coordinate(650000, 650000),
                    new Coordinate(650100, 650000)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
                builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry = lineString.StartPoint;
                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = lineString.EndPoint;
            })
            .WithChange((builder, _) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.Factory.CreateLineString([
                    new Coordinate(650000, 650010),
                    new Coordinate(650100, 650010)
                ]);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
                builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry = lineString.StartPoint;
                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = lineString.EndPoint;
            })
            .BuildWithResult(context =>
            {
                var maxRoadSegmentId = context.GetMaxRoadSegmentId();
                var roadSegment1Id = maxRoadSegmentId.Next();

                return TranslatedChanges.Empty
                    .AppendChange(new RemoveRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                    })
                    .AppendChange(new AddRoadNodeChange
                    {
                        TemporaryId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                        Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                        Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                    })
                    .AppendChange(new RemoveRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                    })
                    .AppendChange(new AddRoadNodeChange
                    {
                        TemporaryId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                        Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                        Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                    })
                    .AppendChange(BuildAddRoadSegmentChange(context.Change.TestData.RoadSegment1DbaseRecord, context.Change.TestData.RoadSegment1ShapeRecord) with
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(roadSegment1Id, [new RoadSegmentTempId(context.Change.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value)]),
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                        EuropeanRoadNumbers =
                        [
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!),
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)
                        ],
                        NationalRoadNumbers =
                        [
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!),
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NWNUMMER.Value!)
                        ]
                    })
                    .AppendChange(new RemoveRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value)
                    })
                    .AppendChange(new AddGradeSeparatedJunctionChange
                    {
                        TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                        UpperRoadSegmentId = roadSegment1Id,
                        LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value),
                        Type = GradeSeparatedJunctionTypeV2.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]
                    })
                    .AppendChange(new RemoveGradeSeparatedJunctionChange
                    {
                        GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                    });
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenModifiedNonCriticalAttribute_ThenModifiedSegment()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentMorphologyV2.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.MORF.Value]);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(BuildModifyRoadSegmentChange(context.Change.TestData.RoadSegment1DbaseRecord, context.Change.TestData.RoadSegment1ShapeRecord) with
                {
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenAddingNewSegmentWith70OverlapToUnchangedExtractSegment_ThenExtractSegmentKeepsId()
    {
        var grbOgcDownloader = new FakeGrbOgcApiFeaturesDownloader();
        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(grbOgcApiFeaturesDownloader: grbOgcDownloader);

        var startPoint = new Point(602000, 602000).WithSrid(WellknownSrids.Lambert08);
        var endPoint = new Point(602100, 602000).WithSrid(WellknownSrids.Lambert08);

        var (zipArchive, context) = new DomainV2ZipArchiveBuilder(fixture => { fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd); })
            .WithExtract((builder, _) =>
            {
                var geometry = new LineString([
                    startPoint.Coordinate,
                    endPoint.Coordinate
                ]).WithSrid(WellknownSrids.Lambert08);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = geometry.ToMultiLineString();

                builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry = startPoint;
                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = endPoint;
            })
            .WithChange((builder, _) =>
            {
                var segment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry;

                var lineStringCloseToOtherButLessOverlapThanExtract = new LineString([
                    new Coordinate(startPoint.X + 5, startPoint.Y + 0.5),
                    new Coordinate(endPoint.X - 5, endPoint.Y + 0.5)
                ]).WithSrid(segment1Geometry.SRID);
                var roadSegmentShapeRecord = builder.CreateRoadSegmentShapeRecord(lineStringCloseToOtherButLessOverlapThanExtract);
                builder.DataSet.RoadSegmentShapeRecords.Add(roadSegmentShapeRecord);
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord());
                builder.DataSet.RoadSegmentDbaseRecords.Last().WS_OIDN.Value = null;

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(lineStringCloseToOtherButLessOverlapThanExtract.StartPoint));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(lineStringCloseToOtherButLessOverlapThanExtract.EndPoint));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());

                grbOgcDownloader.AddRange(builder.DataSet.RoadSegmentShapeRecords.Select(x => x.Geometry));
            })
            .BuildWithContext();

        var changes = await TranslateSucceeds(zipArchive, translator);

        var extractRoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value);

        var addSegmentChange = changes.OfType<AddRoadSegmentChange>().Single();
        addSegmentChange.RoadSegmentIdReference.RoadSegmentId.Should().NotBe(extractRoadSegmentId);
        addSegmentChange.RoadSegmentIdReference.TempIds!.Single().Should().Be(new RoadSegmentTempId(context.Change.DataSet.RoadSegmentDbaseRecords.Last().WS_TEMPID.Value));
    }

    [Fact]
    public async Task WhenAddingNewSegmentWith70OverlapToModifiedExtractSegment_ThenSegmentWithLargestOverlapKeepsExtractRoadSegmentId()
    {
        var grbOgcDownloader = new FakeGrbOgcApiFeaturesDownloader();
        var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(grbOgcApiFeaturesDownloader: grbOgcDownloader);

        var startPoint = new Point(602000, 602000).WithSrid(WellknownSrids.Lambert08);
        var endPoint = new Point(602100, 602000).WithSrid(WellknownSrids.Lambert08);

        var (zipArchive, context) = new DomainV2ZipArchiveBuilder(fixture => { fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd); })
            .WithExtract((builder, _) =>
            {
                var geometry = new LineString([
                    startPoint.Coordinate,
                    endPoint.Coordinate
                ]).WithSrid(WellknownSrids.Lambert08);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = geometry.ToMultiLineString();

                builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry = startPoint;
                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = endPoint;
            })
            .WithChange((builder, _) =>
            {
                var segment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry;

                // modify segment
                var modifiedGeometryButWithLessOverlapThanNewGeometry = new LineString([
                    new Coordinate(startPoint.X + 5, startPoint.Y - 0.5),
                    new Coordinate(endPoint.X - 5, endPoint.Y - 0.5)
                ]).WithSrid(segment1Geometry.SRID);
                var modifiedGeometry = modifiedGeometryButWithLessOverlapThanNewGeometry.ToMultiLineString();
                builder.TestData.RoadSegment1ShapeRecord.Geometry = modifiedGeometry;

                // add segment
                var lineStringCloseToOtherButMoreOverlapWithExtract = new LineString([
                    new Coordinate(startPoint.X - 5, startPoint.Y + 0.5),
                    new Coordinate(endPoint.X + 5, endPoint.Y + 0.5)
                ]).WithSrid(segment1Geometry.SRID);
                var roadSegmentShapeRecord = builder.CreateRoadSegmentShapeRecord(lineStringCloseToOtherButMoreOverlapWithExtract);
                builder.DataSet.RoadSegmentShapeRecords.Add(roadSegmentShapeRecord);
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord());
                builder.DataSet.RoadSegmentDbaseRecords.Last().WS_OIDN.Value = null;

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(lineStringCloseToOtherButMoreOverlapWithExtract.StartPoint));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(lineStringCloseToOtherButMoreOverlapWithExtract.EndPoint));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());

                grbOgcDownloader.AddRange(builder.DataSet.RoadSegmentShapeRecords.Select(x => x.Geometry));
            })
            .BuildWithContext();

        var changes = await TranslateSucceeds(zipArchive, translator);

        var extractRoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value);

        var modifySegmentChange = changes.OfType<ModifyRoadSegmentChange>().Single();
        modifySegmentChange.RoadSegmentIdReference.RoadSegmentId.Should().Be(extractRoadSegmentId);
        modifySegmentChange.RoadSegmentIdReference.TempIds!.Single().Should().Be(new RoadSegmentTempId(context.Change.DataSet.RoadSegmentDbaseRecords.Last().WS_TEMPID.Value));

        var addSegmentChange = changes.OfType<AddRoadSegmentChange>().Single();
        addSegmentChange.RoadSegmentIdReference.RoadSegmentId.Should().NotBe(extractRoadSegmentId);
        addSegmentChange.RoadSegmentIdReference.TempIds!.Single().Should().Be(new RoadSegmentTempId(context.Change.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value));
    }

    [Fact]
    public async Task WhenConversionToGerealiseerd_ThenSuccess()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatusV2.Gepland;

                builder.DataSet.RoadNodeDbaseRecords.Remove(builder.TestData.RoadSegment1StartNodeDbaseRecord);
                builder.DataSet.RoadNodeDbaseRecords.Remove(builder.TestData.RoadSegment1EndNodeDbaseRecord);
                builder.DataSet.RoadNodeShapeRecords.Remove(builder.TestData.RoadSegment1StartNodeShapeRecord);
                builder.DataSet.RoadNodeShapeRecords.Remove(builder.TestData.RoadSegment1EndNodeShapeRecord);
            })
            .WithChange((builder, _) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatusV2.Gerealiseerd;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                    Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                    Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                })
                .AppendChange(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                    Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                    Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                })
                .AppendChange(BuildModifyRoadSegmentChange(context.Change.TestData.RoadSegment1DbaseRecord, context.Change.TestData.RoadSegment1ShapeRecord) with
                {
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                    Status = RoadSegmentStatusV2.Gerealiseerd
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task GivenMultipleNonGerealiseerdRoadSegmentsWithIdenticalGeometries_WithNoChanges_ThenNoChanges()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                var newSegment1 = builder.CreateRoadSegmentShapeRecord();
                var newSegment2 = builder.CreateRoadSegmentShapeRecord(newSegment1.Geometry);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment2);
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.RoadSegmentDbaseRecords.Add(context.Extract.DataSet.RoadSegmentDbaseRecords[2]);
                builder.DataSet.RoadSegmentDbaseRecords.Add(context.Extract.DataSet.RoadSegmentDbaseRecords[3]);

                builder.DataSet.RoadSegmentShapeRecords.Add(context.Extract.DataSet.RoadSegmentShapeRecords[2]);
                builder.DataSet.RoadSegmentShapeRecords.Add(context.Extract.DataSet.RoadSegmentShapeRecords[3]);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenMultipleNonGerealiseerdRoadSegmentsWithOverlappingGeometriesButChangedGeometries_ThenRoadSegmentModified()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));

                var newSegment1 = builder.CreateRoadSegmentShapeRecord();
                var newSegment2 = builder.CreateRoadSegmentShapeRecord(newSegment1.Geometry);
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1);
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment2);
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.RoadSegmentDbaseRecords.Add(context.Extract.DataSet.RoadSegmentDbaseRecords[2]);
                builder.DataSet.RoadSegmentDbaseRecords.Add(context.Extract.DataSet.RoadSegmentDbaseRecords[3]);

                var lineString = context.Extract.DataSet.RoadSegmentShapeRecords[2].Geometry.GetSingleLineString();
                lineString = lineString.Factory.CreateLineString([
                    lineString.Coordinates[0],
                    new Coordinate(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y)
                ]);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(lineString));
                builder.DataSet.RoadSegmentShapeRecords.Add(context.Extract.DataSet.RoadSegmentShapeRecords[3]);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(BuildModifyRoadSegmentChange(context.Change.DataSet.RoadSegmentDbaseRecords[2], context.Change.DataSet.RoadSegmentShapeRecords[2]) with
                {
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
                })
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task TempIdsShouldBeUniqueAcrossChangeAndIntegrationData()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, context) =>
            {
                var integrationRoadSegment = context.Integration.DataSet.RoadSegmentDbaseRecords.First();

                builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value = integrationRoadSegment.WS_TEMPID.Value;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange), problem.Reason);
    }

    [Fact]
    public async Task GivenTwoGeometricallyIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentAttributesAreModified_ThenSecondIdIsUsed()
    {
        var roadSegment2TempId = 0;

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                var newSegment1 = builder.CreateRoadSegmentShapeRecord();
                var newSegment2 = builder.CreateRoadSegmentShapeRecord(newSegment1.Geometry);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment2);
            })
            .WithChange((builder, context) =>
            {
                // change roadsegment 2
                var roadSegmentShapeRecord = context.Extract.DataSet.RoadSegmentShapeRecords.Last();
                var roadSegmentDbaseRecord = context.Extract.DataSet.RoadSegmentDbaseRecords.Last();
                roadSegmentDbaseRecord.WEGCAT.Value = context.Fixture.CreateWhichIsDifferentThan(
                    RoadSegmentCategoryV2.ByIdentifier[roadSegmentDbaseRecord.WEGCAT.Value]
                ).Translation.Identifier;

                builder.DataSet.RoadSegmentShapeRecords.Add(roadSegmentShapeRecord);
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord);

                roadSegment2TempId = roadSegmentDbaseRecord.WS_TEMPID.Value;
            })
            .Build();

        var translatedChanges = await TranslateSucceeds(zipArchive);

        var modifyRoadSegment2 = translatedChanges.OfType<ModifyRoadSegmentChange>().Single();
        modifyRoadSegment2.RoadSegmentIdReference.TempIds.Should().Contain(new RoadSegmentTempId(roadSegment2TempId));
    }

    [Fact]
    public async Task GivenTwoGeometricallyIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentGeometryIsSlightlyChanged_ThenSecondIdIsUsed()
    {
        var roadSegment2TempId = 0;

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gepland))
            .WithExtract((builder, _) =>
            {
                var newSegment1 = builder.CreateRoadSegmentShapeRecord();
                var newSegment2 = builder.CreateRoadSegmentShapeRecord(newSegment1.Geometry);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment2);
            })
            .WithChange((builder, context) =>
            {
                var roadSegmentShapeRecord = context.Extract.DataSet.RoadSegmentShapeRecords.Last();
                var roadSegmentDbaseRecord = context.Extract.DataSet.RoadSegmentDbaseRecords.Last();
                var lineString = roadSegmentShapeRecord.Geometry.GetSingleLineString();
                lineString = lineString.Factory.CreateLineString([
                    lineString.Coordinates[0],
                    new Coordinate(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y)
                ]);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(lineString));
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord);

                roadSegment2TempId = roadSegmentDbaseRecord.WS_TEMPID.Value;
            })
            .Build();

        var translatedChanges = await TranslateSucceeds(zipArchive);
        var modifyRoadSegment2 = translatedChanges.OfType<ModifyRoadSegmentChange>().Single();
        modifyRoadSegment2.RoadSegmentIdReference.TempIds.Should().Contain(new RoadSegmentTempId(roadSegment2TempId));
    }

    [Fact]
    public async Task GivenTwoGeometricallyIdenticalRoadSegments_WhenFirstRoadSegmentIsRemovedAndSecondRoadSegmentIsUnchanged_ThenOnlyFirstRemoved()
    {
        var roadSegment1Id = 0;

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                var newSegment1 = builder.CreateRoadSegmentShapeRecord();
                var newSegment2 = builder.CreateRoadSegmentShapeRecord(newSegment1.Geometry);

                var newSegment1Dbase = builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland);
                roadSegment1Id = newSegment1Dbase.WS_OIDN.Value!.Value;
                builder.DataSet.RoadSegmentDbaseRecords.Add(newSegment1Dbase);
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record => record.STATUS.Value = RoadSegmentStatusV2.Gepland));
                builder.DataSet.RoadSegmentShapeRecords.Add(newSegment2);
            })
            .WithChange((builder, context) =>
            {
                var roadSegmentShapeRecord = context.Extract.DataSet.RoadSegmentShapeRecords.Last();
                var roadSegmentDbaseRecord = context.Extract.DataSet.RoadSegmentDbaseRecords.Last();

                builder.DataSet.RoadSegmentShapeRecords.Add(roadSegmentShapeRecord);
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord);
            })
            .Build();

        var translatedChanges = await TranslateSucceeds(zipArchive);
        translatedChanges
            .OfType<RemoveRoadSegmentChange>()
            .Should()
            .ContainSingle(change => change.RoadSegmentId == new RoadSegmentId(roadSegment1Id));
    }

    [Fact]
    public async Task WhenOnlyRoadSegmentIdChanges_ThenNoChanges()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithChange((builder, _) =>
            {
                var newSegmentId = builder.DataSet.RoadSegmentDbaseRecords.Select(x => x.WS_OIDN.Value).Max() + 1;
                builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value = newSegmentId;
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenRoadSegmentIsConsumedDuringUnflatten_ThenRoadSegmentIsRemoved()
    {
        var roadSegment1Id = 1;
        var roadSegment2Id = 2;

        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithExtract((builder, _) =>
            {
                ConfigureScenario(builder);
            })
            .WithChange((builder, _) =>
            {
                ConfigureScenario(builder);
            })
            .Build();

        var translatedChanges = await TranslateSucceeds(zipArchive);
        var removeChange = translatedChanges.OfType<RemoveRoadSegmentChange>().Single();
        removeChange.RoadSegmentId.Should().Be(new RoadSegmentId(roadSegment2Id));
        var modifyChange = translatedChanges.OfType<ModifyRoadSegmentChange>().Single();
        modifyChange.RoadSegmentIdReference.RoadSegmentId.Should().Be(new RoadSegmentId(roadSegment1Id));

        void ConfigureScenario(ExtractsZipArchiveExtractDataSetBuilder builder)
        {
            builder.DataSet.Clear();

            var segment1Shape = builder.CreateRoadSegmentShapeRecord(new LineString([new Coordinate(650000, 650000), new Coordinate(651000, 650000)]));
            var segment2Shape = builder.CreateRoadSegmentShapeRecord(new LineString([new Coordinate(651000, 650000), new Coordinate(651003, 650000)]));

            var newSegment1Dbase = builder.CreateRoadSegmentDbaseRecord();
            newSegment1Dbase.WS_OIDN.Value = roadSegment1Id;
            builder.DataSet.RoadSegmentDbaseRecords.Add(newSegment1Dbase);
            builder.DataSet.RoadSegmentShapeRecords.Add(segment1Shape);

            var newSegment2Dbase = newSegment1Dbase.Clone(new RecyclableMemoryStreamManager(), Encoding.UTF8);
            newSegment2Dbase.WS_OIDN.Value = roadSegment2Id;
            newSegment2Dbase.WS_TEMPID.Value++;
            builder.DataSet.RoadSegmentDbaseRecords.Add(newSegment2Dbase);
            builder.DataSet.RoadSegmentShapeRecords.Add(segment2Shape);

            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());
            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());
            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());
            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(segment1Shape.Geometry.GetSingleLineString().StartPoint));
            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(segment1Shape.Geometry.GetSingleLineString().EndPoint));
            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(segment2Shape.Geometry.GetSingleLineString().EndPoint));
        }
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
