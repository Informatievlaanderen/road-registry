namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Exceptions;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Xunit.Abstractions;

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
        var organizationRepository = new FakeOrganizationRepository()
            .Seed(orgId, null);

        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.BEHEER.Value = orgId;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        var translator = ZipArchiveFeatureCompareTranslatorFactory.Create(Logger, organizationRepository);
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateSucceeds(zipArchive, translator));
        Assert.NotEmpty(ex.Problems);
        Assert.True(ex.Problems.All(x => x.Reason == "RoadSegmentMaintenanceAuthorityNotKnown"));
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
                .AppendChange(new ModifyRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                    new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                    new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEER.Value),
                    RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                    RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORF.Value],
                    RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                    RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.WEGCAT.Value],
                    RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                    CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                    CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
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
                )));

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
                            CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                            CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
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
                        roadSegment1Id,
                        EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToEuropeanRoad(
                        new RecordNumber(2),
                        new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value),
                        roadSegment1Id,
                        EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                        new RecordNumber(2),
                        new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToNationalRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                        roadSegment1Id,
                        NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToNationalRoad(
                        new RecordNumber(2),
                        new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value),
                        roadSegment1Id,
                        NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromNationalRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromNationalRoad(
                        new RecordNumber(2),
                        new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToNumberedRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                        roadSegment1Id,
                        NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value),
                        RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.RICHTING.Value],
                        new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.VOLGNUMMER.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToNumberedRoad(
                        new RecordNumber(2),
                        new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.GW_OIDN.Value),
                        roadSegment1Id,
                        NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value),
                        RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.RICHTING.Value],
                        new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.VOLGNUMMER.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                        new RecordNumber(2),
                        new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.GW_OIDN.Value),
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
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
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
                        new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                        new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                        new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEER.Value),
                        RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                        RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORF.Value],
                        RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                        RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.WEGCAT.Value],
                        RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                        CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                        CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
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
                    ));
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
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 0.1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 0.1)
                });
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
                            new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                            new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                            new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value],
                            RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                            RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value],
                            RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                            CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                            CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
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

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.Clear();

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment1DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment1ShapeRecord);

                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
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
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
            })
            .WithChange((builder, context) =>
            {
                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 1)
                });
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
                
                builder.DataSet.Clear();
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment1DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment1ShapeRecord);
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.TestData.RoadSegment2DbaseRecord);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.TestData.RoadSegment2ShapeRecord);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new ModifyRoadSegment(
                            new RecordNumber(1),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                            new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                            new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEERDER.Value),
                            RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                            RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORFOLOGIE.Value],
                            RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                            RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.CATEGORIE.Value],
                            RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                            CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                            CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
                        )
                        .WithGeometry(context.Change.TestData.RoadSegment1ShapeRecord.Geometry)
                )
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
}
