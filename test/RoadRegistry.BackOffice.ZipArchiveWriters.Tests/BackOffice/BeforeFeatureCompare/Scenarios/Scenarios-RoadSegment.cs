namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Exceptions;
using FeatureCompare;
using Microsoft.Extensions.Logging;
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
    public async Task ModifiedGeometrySlightly()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var lineString = GeometryTranslator.ToMultiLineString(builder.TestData.RoadSegment1ShapeRecord.Shape).GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 1)
                });
                builder.TestData.RoadSegment1ShapeRecord = lineString.ToShapeContent();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment1ShapeRecord, builder.TestData.RoadSegment2ShapeRecord }.ToList();

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
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
                .WithGeometry(GeometryTranslator.ToMultiLineString(context.Change.TestData.RoadSegment1ShapeRecord.Shape))
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
                builder.TestData.RoadSegment1ShapeRecord = lineString.ToShapeContent();
                builder.DataSet.RoadSegmentShapeRecords[0] = builder.TestData.RoadSegment1ShapeRecord;
            })
            .WithChange((builder, context) =>
            {
                var lineString = GeometryTranslator.ToMultiLineString(builder.TestData.RoadSegment1ShapeRecord.Shape).GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 9000, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 9000)
                });
                builder.TestData.RoadSegment1ShapeRecord = lineString.ToShapeContent();
                builder.DataSet.RoadSegmentShapeRecords[0] = builder.TestData.RoadSegment1ShapeRecord;

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
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
                        .WithGeometry(GeometryTranslator.ToMultiLineString(context.Change.TestData.RoadSegment1ShapeRecord.Shape))
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
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToEuropeanRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord.EU_OIDN.Value),
                        roadSegment1Id,
                        EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord.EUNUMMER.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord.EU_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord.EUNUMMER.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToNationalRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord.NW_OIDN.Value),
                        roadSegment1Id,
                        NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord.IDENT2.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromNationalRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord.NW_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord.IDENT2.Value)
                    ))
                    .AppendChange(new AddRoadSegmentToNumberedRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord.GW_OIDN.Value),
                        roadSegment1Id,
                        NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord.IDENT8.Value),
                        RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord.RICHTING.Value],
                        new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord.VOLGNUMMER.Value)
                    ))
                    .AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                        new RecordNumber(1),
                        new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord.GW_OIDN.Value),
                        new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord.IDENT8.Value)
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
                    .WithGeometry(GeometryTranslator.ToMultiLineString(context.Change.TestData.RoadSegment1ShapeRecord.Shape))
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

                var lineString = GeometryTranslator.ToMultiLineString(builder.TestData.RoadSegment1ShapeRecord.Shape).GetSingleLineString();
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(lineString.Coordinates[1].X + 0.1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M + 0.1)
                });
                var roadSegmentShapeRecord = lineString.ToShapeContent();
                builder.DataSet.RoadSegmentShapeRecords.Add(roadSegmentShapeRecord);
                
                var roadSegmentDbaseRecord = builder.CreateRoadSegmentDbaseRecord();
                roadSegmentDbaseRecord.B_WK_OIDN.Value = roadNodeDbaseRecord1.WK_OIDN.Value;
                roadSegmentDbaseRecord.E_WK_OIDN.Value = roadNodeDbaseRecord2.WK_OIDN.Value;
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord);
                
                var laneDbaseRecord = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord.WS_OIDN.Value;
                laneDbaseRecord.VANPOS.Value = roadSegmentShapeRecord.Shape.MeasureRange.Min;
                laneDbaseRecord.TOTPOS.Value = roadSegmentShapeRecord.Shape.MeasureRange.Max;
                builder.DataSet.LaneDbaseRecords.Add(laneDbaseRecord);

                var surfaceDbaseRecord = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeRecord.Shape.MeasureRange.Min;
                surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeRecord.Shape.MeasureRange.Max;
                builder.DataSet.SurfaceDbaseRecords.Add(surfaceDbaseRecord);

                var widthDbaseRecord = builder.CreateRoadSegmentWidthDbaseRecord();
                widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord.WS_OIDN.Value;
                widthDbaseRecord.VANPOS.Value = roadSegmentShapeRecord.Shape.MeasureRange.Min;
                widthDbaseRecord.TOTPOS.Value = roadSegmentShapeRecord.Shape.MeasureRange.Max;
                builder.DataSet.WidthDbaseRecords.Add(widthDbaseRecord);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadSegmentIsAlreadyProcessed), problem.Reason);
    }
}
