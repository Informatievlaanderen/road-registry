namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Xunit.Abstractions;

public class AllScenarios : FeatureCompareTranslatorScenariosBase
{
    public AllScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task Added()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                builder.DataSet.RoadNodeDbaseRecords = new[] { builder.TestData.RoadNode3DbaseRecord, builder.TestData.RoadNode4DbaseRecord }.ToList();
                builder.DataSet.RoadNodeShapeRecords = new[] { builder.TestData.RoadNode3ShapeRecord, builder.TestData.RoadNode4ShapeRecord }.ToList();
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment2DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment2ShapeRecord }.ToList();
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.NumberedRoadDbaseRecords.Clear();
                builder.DataSet.LaneDbaseRecords = new[] { builder.TestData.RoadSegment2LaneDbaseRecord }.ToList();
                builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment2SurfaceDbaseRecord }.ToList();
                builder.DataSet.WidthDbaseRecords = new[] { builder.TestData.RoadSegment2WidthDbaseRecord }.ToList();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(context =>
            {
                var maxRoadSegmentId = context.GetMaxRoadSegmentId();
                var roadSegment1TemporaryId = maxRoadSegmentId.Next();

                return TranslatedChanges.Empty
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
                        new AddRoadSegment(
                            new RecordNumber(1),
                            roadSegment1TemporaryId,
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
                        ).WithGeometry(context.Change.TestData.RoadSegment1ShapeRecord.Geometry)
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
                        new AddRoadSegmentToEuropeanRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                            roadSegment1TemporaryId,
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToEuropeanRoad
                        (
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value),
                            roadSegment1TemporaryId,
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToNationalRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                            roadSegment1TemporaryId,
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToNationalRoad
                        (
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value),
                            roadSegment1TemporaryId,
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToNumberedRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                            roadSegment1TemporaryId,
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value),
                            RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.RICHTING.Value],
                            new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.VOLGNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToNumberedRoad
                        (
                            new RecordNumber(2),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.GW_OIDN.Value),
                            roadSegment1TemporaryId,
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value),
                            RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.RICHTING.Value],
                            new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord2.VOLGNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new AddGradeSeparatedJunction
                        (
                            new RecordNumber(1),
                            new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            GradeSeparatedJunctionType.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            roadSegment1TemporaryId,
                            new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value)
                        )
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Modified()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.RoadNode1DbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadNodeType.ByIdentifier[builder.TestData.RoadNode1DbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.STATUS.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value = fixture.CreateWhichIsDifferentThan(EuropeanRoadNumber.Parse(builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)).ToString();
                builder.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value = fixture.CreateWhichIsDifferentThan(NationalRoadNumber.Parse(builder.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)).ToString();
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value = fixture.CreateWhichIsDifferentThan(NumberedRoadNumber.Parse(builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value)).ToString();
                builder.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentLaneDirection.ByIdentifier[builder.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value = fixture.CreateWhichIsDifferentThan(new RoadSegmentWidth(builder.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value));
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentSurfaceType.ByIdentifier[builder.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new ModifyRoadNode(
                            new RecordNumber(1),
                            new RoadNodeId(context.Change.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                            RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode1DbaseRecord.TYPE.Value]
                        ).WithGeometry(context.Change.TestData.RoadNode1ShapeRecord.Geometry)
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
                        new AddRoadSegmentToEuropeanRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromEuropeanRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToNationalRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                        )
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromNationalRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                        )
                    )
                    .AppendChange(
                        new AddRoadSegmentToNumberedRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NumberedRoadNumber.Parse(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value),
                            RoadSegmentNumberedRoadDirection.ByIdentifier[context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.RICHTING.Value],
                            new RoadSegmentNumberedRoadOrdinal(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.VOLGNUMMER.Value)
                        )
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromNumberedRoad
                        (
                            new RecordNumber(1),
                            new AttributeId(context.Change.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                            new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                            NumberedRoadNumber.Parse(context.Extract.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value)
                        )
                    )
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
    public async Task NoChanges()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .BuildWithResult(_ => TranslatedChanges.Empty);
        
        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Removed()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.DataSet.RoadNodeDbaseRecords = new[] { builder.TestData.RoadNode3DbaseRecord, builder.TestData.RoadNode4DbaseRecord }.ToList();
                builder.DataSet.RoadNodeShapeRecords = new[] { builder.TestData.RoadNode3ShapeRecord, builder.TestData.RoadNode4ShapeRecord }.ToList();
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment2DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment2ShapeRecord }.ToList();
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.NumberedRoadDbaseRecords.Clear();
                builder.DataSet.LaneDbaseRecords = new[] { builder.TestData.RoadSegment2LaneDbaseRecord }.ToList();
                builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment2SurfaceDbaseRecord }.ToList();
                builder.DataSet.WidthDbaseRecords = new[] { builder.TestData.RoadSegment2WidthDbaseRecord }.ToList();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
            .AppendChange(
                new RemoveRoadNode(
                    new RecordNumber(1),
                    new RoadNodeId(context.Extract.TestData.RoadNode1DbaseRecord.WK_OIDN.Value))
            )
            .AppendChange(
                new RemoveRoadNode(
                    new RecordNumber(2),
                    new RoadNodeId(context.Extract.TestData.RoadNode2DbaseRecord.WK_OIDN.Value))
            )
            .AppendChange(
                new RemoveRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromEuropeanRoad
                (
                    new RecordNumber(1),
                    new AttributeId(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromEuropeanRoad
                (
                    new RecordNumber(2),
                    new AttributeId(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNationalRoad
                (
                    new RecordNumber(1),
                    new AttributeId(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNationalRoad
                (
                    new RecordNumber(2),
                    new AttributeId(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNumberedRoad
                (
                    new RecordNumber(1),
                    new AttributeId(context.Extract.TestData.RoadSegment1NumberedRoadDbaseRecord1.GW_OIDN.Value),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    NumberedRoadNumber.Parse(context.Extract.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNumberedRoad
                (
                    new RecordNumber(2),
                    new AttributeId(context.Extract.TestData.RoadSegment1NumberedRoadDbaseRecord2.GW_OIDN.Value),
                    new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    NumberedRoadNumber.Parse(context.Extract.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value)
                )
            )
            .AppendChange(
                new RemoveGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                )
            ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
