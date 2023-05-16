namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Exceptions;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using Uploads;

public class FeatureCompareTranslatorScenarios
{
    private static readonly Encoding Encoding = Encoding.UTF8;
    private readonly ILogger<ZipArchiveFeatureCompareTranslator> _logger;

    public FeatureCompareTranslatorScenarios(ILogger<ZipArchiveFeatureCompareTranslator> logger)
    {
        _logger = logger;
    }

    [Fact]
    public async Task All_Added()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream,
            roadSegmentDbaseChangeStream,
            roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream,
            roadNodeDbaseChangeStream,
            europeanRoadChangeStream,
            numberedRoadChangeStream,
            nationalRoadChangeStream,
            laneChangeStream,
            widthChangeStream,
            surfaceChangeStream,
            gradeSeparatedJunctionChangeStream
        );

        var maxRoadSegmentId = Math.Max(roadSegmentDbaseRecord1.WS_OIDN.Value, roadSegmentDbaseRecord2.WS_OIDN.Value);
        var roadSegmentTemporaryId1 = new RoadSegmentId(maxRoadSegmentId + 1);
        var roadSegmentTemporaryId2 = new RoadSegmentId(maxRoadSegmentId + 2);

        var expected = TranslatedChanges.Empty
            .AppendChange(
                new AddRoadNode(
                    new RecordNumber(1),
                    new RoadNodeId(roadNodeDbaseRecord1.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord1.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent1.Shape))
            )
            .AppendChange(
                new AddRoadNode(
                    new RecordNumber(2),
                    new RoadNodeId(roadNodeDbaseRecord2.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord2.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent2.Shape))
            )
            .AppendChange(
                new AddRoadNode(
                    new RecordNumber(3),
                    new RoadNodeId(roadNodeDbaseRecord3.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord3.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent3.Shape))
            )
            .AppendChange(
                new AddRoadNode(
                    new RecordNumber(4),
                    new RoadNodeId(roadNodeDbaseRecord4.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord4.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent4.Shape))
            )
            .AppendChange(
                new AddRoadSegment(
                        new RecordNumber(1),
                        roadSegmentTemporaryId1,
                        new RoadNodeId(roadSegmentDbaseRecord1.B_WK_OIDN.Value),
                        new RoadNodeId(roadSegmentDbaseRecord1.E_WK_OIDN.Value),
                        new OrganizationId(roadSegmentDbaseRecord1.BEHEERDER.Value),
                        RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord1.METHODE.Value],
                        RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord1.MORFOLOGIE.Value],
                        RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord1.STATUS.Value],
                        RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord1.CATEGORIE.Value],
                        RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord1.TGBEP.Value],
                        CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.LSTRNMID.Value),
                        CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.RSTRNMID.Value)
                    )
                    .WithGeometry(GeometryTranslator.ToMultiLineString(roadSegmentShapeContent1.Shape))
                    .WithLane(
                        new RoadSegmentLaneAttribute(
                            new AttributeId(laneDbaseRecord.RS_OIDN.Value),
                            new RoadSegmentLaneCount(laneDbaseRecord.AANTAL.Value),
                            RoadSegmentLaneDirection.ByIdentifier[laneDbaseRecord.RICHTING.Value],
                            new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.TOTPOS.Value))
                        )
                    )
                    .WithWidth(
                        new RoadSegmentWidthAttribute(
                            new AttributeId(widthDbaseRecord.WB_OIDN.Value),
                            new RoadSegmentWidth(widthDbaseRecord.BREEDTE.Value),
                            new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.TOTPOS.Value))
                        )
                    )
                    .WithSurface(
                        new RoadSegmentSurfaceAttribute(
                            new AttributeId(surfaceDbaseRecord.WV_OIDN.Value),
                            RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord.TYPE.Value],
                            new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.TOTPOS.Value))
                        )
                    )
            )
            .AppendChange(
                new AddRoadSegment(
                    new RecordNumber(2),
                    roadSegmentTemporaryId2,
                    new RoadNodeId(roadSegmentDbaseRecord2.B_WK_OIDN.Value),
                    new RoadNodeId(roadSegmentDbaseRecord2.E_WK_OIDN.Value),
                    new OrganizationId(roadSegmentDbaseRecord2.BEHEERDER.Value),
                    RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord2.METHODE.Value],
                    RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord2.MORFOLOGIE.Value],
                    RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord2.STATUS.Value],
                    RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord2.CATEGORIE.Value],
                    RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord2.TGBEP.Value],
                    CrabStreetnameId.FromValue(roadSegmentDbaseRecord2.LSTRNMID.Value),
                    CrabStreetnameId.FromValue(roadSegmentDbaseRecord2.RSTRNMID.Value)
                ).WithGeometry(GeometryTranslator.ToMultiLineString(roadSegmentShapeContent2.Shape))
            )
            .AppendChange(
                new AddRoadSegmentToEuropeanRoad
                (
                    new RecordNumber(1),
                    new AttributeId(europeanRoadDbaseRecord.EU_OIDN.Value),
                    roadSegmentTemporaryId1,
                    EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value)
                )
            )
            .AppendChange(
                new AddRoadSegmentToNationalRoad
                (
                    new RecordNumber(1),
                    new AttributeId(nationalRoadDbaseRecord.NW_OIDN.Value),
                    roadSegmentTemporaryId1,
                    NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value)
                )
            )
            .AppendChange(
                new AddRoadSegmentToNumberedRoad
                (
                    new RecordNumber(1),
                    new AttributeId(numberedRoadDbaseRecord.GW_OIDN.Value),
                    roadSegmentTemporaryId1,
                    NumberedRoadNumber.Parse(numberedRoadDbaseRecord.IDENT8.Value),
                    RoadSegmentNumberedRoadDirection.ByIdentifier[numberedRoadDbaseRecord.RICHTING.Value],
                    new RoadSegmentNumberedRoadOrdinal(numberedRoadDbaseRecord.VOLGNUMMER.Value)
                )
            )
            .AppendChange(
                new AddGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                    GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value],
                    roadSegmentTemporaryId1,
                    roadSegmentTemporaryId2
                )
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task All_Modified()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        roadSegmentDbaseRecord1.STATUS.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord1.STATUS.Value]).Translation.Identifier;
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadExtractStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var europeanRoadDbaseRecordExtractEuropeanRoadNumber = EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value);
        europeanRoadDbaseRecord.EUNUMMER.Value = fixture.CreateWhichIsDifferentThan(EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value)).ToString();
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadExtractStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var nationalRoadDbaseRecordExtractNationalRoadNumber = NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value);
        nationalRoadDbaseRecord.IDENT2.Value = fixture.CreateWhichIsDifferentThan(NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value)).ToString();
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadExtractStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var numberedRoadDbaseRecordExtractNumberedRoadNumber = NumberedRoadNumber.Parse(numberedRoadDbaseRecord.IDENT8.Value);
        numberedRoadDbaseRecord.IDENT8.Value = fixture.CreateWhichIsDifferentThan(NumberedRoadNumber.Parse(numberedRoadDbaseRecord.IDENT8.Value)).ToString();
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        laneDbaseRecord.RICHTING.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentLaneDirection.ByIdentifier[laneDbaseRecord.RICHTING.Value]).Translation.Identifier;
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        widthDbaseRecord.BREEDTE.Value = fixture.CreateWhichIsDifferentThan(new RoadSegmentWidth(widthDbaseRecord.BREEDTE.Value));
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        surfaceDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord.TYPE.Value]).Translation.Identifier;
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        roadNodeDbaseRecord1.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadNodeType.ByIdentifier[roadNodeDbaseRecord1.TYPE.Value]).Translation.Identifier;
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        gradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            europeanRoadExtractStream: europeanRoadExtractStream,
            europeanRoadChangeStream: europeanRoadChangeStream,
            numberedRoadExtractStream: numberedRoadExtractStream,
            numberedRoadChangeStream: numberedRoadChangeStream,
            nationalRoadExtractStream: nationalRoadExtractStream,
            nationalRoadChangeStream: nationalRoadChangeStream,
            laneExtractStream: laneExtractStream,
            laneChangeStream: laneChangeStream,
            widthExtractStream: widthExtractStream,
            widthChangeStream: widthChangeStream,
            surfaceExtractStream: surfaceExtractStream,
            surfaceChangeStream: surfaceChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var expected = TranslatedChanges.Empty
            .AppendChange(
                new ModifyRoadNode(
                    new RecordNumber(1),
                    new RoadNodeId(roadNodeDbaseRecord1.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord1.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent1.Shape))
            )
            .AppendChange(
                new ModifyRoadSegment(
                        new RecordNumber(1),
                        new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                        new RoadNodeId(roadSegmentDbaseRecord1.B_WK_OIDN.Value),
                        new RoadNodeId(roadSegmentDbaseRecord1.E_WK_OIDN.Value),
                        new OrganizationId(roadSegmentDbaseRecord1.BEHEERDER.Value),
                        RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord1.METHODE.Value],
                        RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord1.MORFOLOGIE.Value],
                        RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord1.STATUS.Value],
                        RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord1.CATEGORIE.Value],
                        RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord1.TGBEP.Value],
                        CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.LSTRNMID.Value),
                        CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.RSTRNMID.Value)
                    )
                    .WithGeometry(GeometryTranslator.ToMultiLineString(roadSegmentShapeContent1.Shape))
                    .WithLane(
                        new RoadSegmentLaneAttribute(
                            new AttributeId(laneDbaseRecord.RS_OIDN.Value),
                            new RoadSegmentLaneCount(laneDbaseRecord.AANTAL.Value),
                            RoadSegmentLaneDirection.ByIdentifier[laneDbaseRecord.RICHTING.Value],
                            new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.TOTPOS.Value))
                        )
                    )
                    .WithWidth(
                        new RoadSegmentWidthAttribute(
                            new AttributeId(widthDbaseRecord.WB_OIDN.Value),
                            new RoadSegmentWidth(widthDbaseRecord.BREEDTE.Value),
                            new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.TOTPOS.Value))
                        )
                    )
                    .WithSurface(
                        new RoadSegmentSurfaceAttribute(
                            new AttributeId(surfaceDbaseRecord.WV_OIDN.Value),
                            RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord.TYPE.Value],
                            new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.TOTPOS.Value))
                        )
                    )
            )
            .AppendChange(
                new AddRoadSegmentToEuropeanRoad
                (
                    new RecordNumber(1),
                    new AttributeId(europeanRoadDbaseRecord.EU_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromEuropeanRoad
                (
                    new RecordNumber(1),
                    new AttributeId(europeanRoadDbaseRecord.EU_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    europeanRoadDbaseRecordExtractEuropeanRoadNumber
                )
            )
            .AppendChange(
                new AddRoadSegmentToNationalRoad
                (
                    new RecordNumber(1),
                    new AttributeId(nationalRoadDbaseRecord.NW_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNationalRoad
                (
                    new RecordNumber(1),
                    new AttributeId(nationalRoadDbaseRecord.NW_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    nationalRoadDbaseRecordExtractNationalRoadNumber
                )
            )
            .AppendChange(
                new AddRoadSegmentToNumberedRoad
                (
                    new RecordNumber(1),
                    new AttributeId(numberedRoadDbaseRecord.GW_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    NumberedRoadNumber.Parse(numberedRoadDbaseRecord.IDENT8.Value),
                    RoadSegmentNumberedRoadDirection.ByIdentifier[numberedRoadDbaseRecord.RICHTING.Value],
                    new RoadSegmentNumberedRoadOrdinal(numberedRoadDbaseRecord.VOLGNUMMER.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNumberedRoad
                (
                    new RecordNumber(1),
                    new AttributeId(numberedRoadDbaseRecord.GW_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    numberedRoadDbaseRecordExtractNumberedRoadNumber
                )
            )
            .AppendChange(
                new AddGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                    GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value],
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord2.WS_OIDN.Value)
                )
            )
            .AppendChange(
                new RemoveGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                )
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task All_NoChanges()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadExtractStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var europeanRoadDbaseRecordExtractEuropeanRoadNumber = EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value);
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadExtractStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var nationalRoadDbaseRecordExtractNationalRoadNumber = NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value);
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadExtractStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var numberedRoadDbaseRecordExtractNumberedRoadNumber = NumberedRoadNumber.Parse(numberedRoadDbaseRecord.IDENT8.Value);
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            europeanRoadExtractStream: europeanRoadExtractStream,
            europeanRoadChangeStream: europeanRoadChangeStream,
            numberedRoadExtractStream: numberedRoadExtractStream,
            numberedRoadChangeStream: numberedRoadChangeStream,
            nationalRoadExtractStream: nationalRoadExtractStream,
            nationalRoadChangeStream: nationalRoadChangeStream,
            laneExtractStream: laneExtractStream,
            laneChangeStream: laneChangeStream,
            widthExtractStream: widthExtractStream,
            widthChangeStream: widthChangeStream,
            surfaceExtractStream: surfaceExtractStream,
            surfaceChangeStream: surfaceChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var expected = TranslatedChanges.Empty;

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task All_Removed()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var europeanRoadExtractStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var nationalRoadExtractStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        var numberedRoadExtractStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;

        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            europeanRoadExtractStream: europeanRoadExtractStream,
            numberedRoadExtractStream: numberedRoadExtractStream,
            nationalRoadExtractStream: nationalRoadExtractStream,
            laneExtractStream: laneExtractStream,
            widthExtractStream: widthExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream
        );

        var expected = TranslatedChanges.Empty
            .AppendChange(
                new RemoveRoadNode(
                    new RecordNumber(1),
                    new RoadNodeId(roadNodeDbaseRecord1.WK_OIDN.Value)
                )
            )
            .AppendChange(
                new RemoveRoadNode(
                    new RecordNumber(2),
                    new RoadNodeId(roadNodeDbaseRecord2.WK_OIDN.Value))
            )
            .AppendChange(
                new RemoveRoadNode(
                    new RecordNumber(3),
                    new RoadNodeId(roadNodeDbaseRecord3.WK_OIDN.Value))
            )
            .AppendChange(
                new RemoveRoadNode(
                    new RecordNumber(4),
                    new RoadNodeId(roadNodeDbaseRecord4.WK_OIDN.Value))
            )
            .AppendChange(
                new RemoveRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegment(
                    new RecordNumber(2),
                    new RoadSegmentId(roadSegmentDbaseRecord2.WS_OIDN.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromEuropeanRoad
                (
                    new RecordNumber(1),
                    new AttributeId(europeanRoadDbaseRecord.EU_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNationalRoad
                (
                    new RecordNumber(1),
                    new AttributeId(nationalRoadDbaseRecord.NW_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value)
                )
            )
            .AppendChange(
                new RemoveRoadSegmentFromNumberedRoad
                (
                    new RecordNumber(1),
                    new AttributeId(numberedRoadDbaseRecord.GW_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    NumberedRoadNumber.Parse(numberedRoadDbaseRecord.IDENT8.Value)
                )
            )
            .AppendChange(
                new RemoveGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                )
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    private static Fixture CreateFixture(ExtractsZipArchiveTestData testData)
    {
        var fixture = testData.Fixture;

        fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<GradeSeparatedJunctionDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE =
                        { Value = (short)fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                    BO_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
                {
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT2 = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentNumberedRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentNumberedRoadAttributeDbaseRecord
                {
                    GW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT8 = { Value = fixture.Create<NumberedRoadNumber>().ToString() },
                    RICHTING =
                    {
                        Value = (short)fixture.Create<RoadSegmentNumberedRoadDirection>().Translation
                            .Identifier
                    },
                    VOLGNUMMER = { Value = fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadNodeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadNodeDbaseRecord
                {
                    TYPE = { Value = (short)fixture.Create<RoadNodeType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                    METHODE =
                    {
                        Value = (short)fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier
                    },
                    BEHEERDER = { Value = fixture.Create<OrganizationId>() },
                    MORFOLOGIE =
                        { Value = (short)fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                    STATUS = { Value = fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                    CATEGORIE = { Value = fixture.Create<RoadSegmentCategory>().Translation.Identifier },
                    B_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    E_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    LSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    RSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    TGBEP =
                    {
                        Value = (short)fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier
                    }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentLaneAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentLaneAttributeDbaseRecord
                {
                    RS_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    AANTAL = { Value = (short)fixture.Create<RoadSegmentLaneCount>().ToInt32() },
                    RICHTING =
                    {
                        Value = (short)fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier
                    }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentSurfaceAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentSurfaceAttributeDbaseRecord
                {
                    WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TYPE = { Value = (short)fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentWidthAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentWidthAttributeDbaseRecord
                {
                    WB_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOS = { Value = fixture.Create<RoadSegmentPosition>().ToDouble() },
                    BREEDTE = { Value = (short)fixture.Create<RoadSegmentWidth>().ToInt32() }
                })
                .OmitAutoProperties());

        return fixture;
    }

    [Fact]
    public async Task EmptyArchiveShouldHaveNoChanges()
    {
        var testData = new ExtractsZipArchiveTestData();

        await TranslateReturnsExpectedResult(testData.ZipArchiveWithEmptyFiles, TranslatedChanges.Empty);
    }

    [Fact]
    public void EncodingCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveFeatureCompareTranslator(null, null));
    }

    [Fact]
    public async Task GradeSeperatedJunction_Identical()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var expected = TranslatedChanges.Empty;

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task GradeSeperatedJunction_RemovedRoadSegmentShouldGiveError()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1 });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        await Assert.ThrowsAsync<RoadSegmentNotFoundInZipArchiveException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
    }

    [Fact]
    public async Task GradeSeperatedJunction_UnknownRoadSegmentShouldGiveError()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = fixture.CreateWhichIsDifferentThan(roadSegmentDbaseRecord1.WS_OIDN.Value, roadSegmentDbaseRecord2.WS_OIDN.Value);
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = fixture.CreateWhichIsDifferentThan(roadSegmentDbaseRecord1.WS_OIDN.Value, roadSegmentDbaseRecord2.WS_OIDN.Value, gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value);
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        await Assert.ThrowsAsync<RoadSegmentNotFoundInZipArchiveException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
    }

    [Fact]
    public async Task GradeSeperatedJunction_Updated_DifferentId()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord1 = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord1.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord1.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord1 });

        var gradeSeparatedJunctionDbaseRecord2 = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord2.BO_WS_OIDN.Value = gradeSeparatedJunctionDbaseRecord1.BO_WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord2.ON_WS_OIDN.Value = gradeSeparatedJunctionDbaseRecord1.ON_WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value = fixture.CreateWhichIsDifferentThan(new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord1.OK_OIDN.Value));
        gradeSeparatedJunctionDbaseRecord2.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord1.TYPE.Value]).Translation.Identifier;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord2 });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var expected = TranslatedChanges.Empty
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
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord1.OK_OIDN.Value)
                )
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task GradeSeperatedJunction_Updated_SameId()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionExtractStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        gradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            gradeSeparatedJunctionExtractStream: gradeSeparatedJunctionExtractStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var expected = TranslatedChanges.Empty
            .AppendChange(
                new AddGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                    GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord.TYPE.Value],
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    new RoadSegmentId(roadSegmentDbaseRecord2.WS_OIDN.Value)
                )
            )
            .AppendChange(
                new RemoveGradeSeparatedJunction
                (
                    new RecordNumber(1),
                    new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                )
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public void IsZipArchiveFeatureCompareTranslator()
    {
        var sut = new ZipArchiveFeatureCompareTranslator(Encoding, _logger);

        Assert.IsAssignableFrom<IZipArchiveFeatureCompareTranslator>(sut);
    }

    [Fact]
    public async Task RoadSegment_ModifiedGeometrySlightly()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        // extract
        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();

        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1 });
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1 });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        // change
        var geometry = GeometryTranslator.ToMultiLineString(roadSegmentShapeContent1.Shape);
        var lineString = (LineString)geometry[0];
        lineString = new LineString(new[]
        {
            lineString.Coordinates[0],
            new CoordinateM(lineString.Coordinates[1].X + 1, lineString.Coordinates[1].Y, lineString.Coordinates[1].M)
        });
        geometry = new MultiLineString(new[] { lineString });
        var roadSegmentShapeContent1Changed = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1Changed });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1 });

        // assert
        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: fixture.CreateProjectionFormatFileWithOneRecord(),
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            laneExtractStream: laneExtractStream,
            widthExtractStream: widthExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            laneChangeStream: laneExtractStream,
            widthChangeStream: widthExtractStream,
            surfaceChangeStream: surfaceExtractStream
        );

        var expected = TranslatedChanges.Empty
            .AppendChange(new ModifyRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    new RoadNodeId(roadSegmentDbaseRecord1.B_WK_OIDN.Value),
                    new RoadNodeId(roadSegmentDbaseRecord1.E_WK_OIDN.Value),
                    new OrganizationId(roadSegmentDbaseRecord1.BEHEER.Value),
                    RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord1.METHODE.Value],
                    RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord1.MORF.Value],
                    RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord1.STATUS.Value],
                    RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord1.WEGCAT.Value],
                    RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord1.TGBEP.Value],
                    CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.LSTRNMID.Value),
                    CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.RSTRNMID.Value)
                )
                .WithGeometry(GeometryTranslator.ToMultiLineString(roadSegmentShapeContent1Changed.Shape))
                .WithLane(
                    new RoadSegmentLaneAttribute(
                        new AttributeId(laneDbaseRecord.RS_OIDN.Value),
                        new RoadSegmentLaneCount(laneDbaseRecord.AANTAL.Value),
                        RoadSegmentLaneDirection.ByIdentifier[laneDbaseRecord.RICHTING.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithWidth(
                    new RoadSegmentWidthAttribute(
                        new AttributeId(widthDbaseRecord.WB_OIDN.Value),
                        new RoadSegmentWidth(widthDbaseRecord.BREEDTE.Value),
                        new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithSurface(
                    new RoadSegmentSurfaceAttribute(
                        new AttributeId(surfaceDbaseRecord.WV_OIDN.Value),
                        RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord.TYPE.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.TOTPOS.Value))
                    )
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task RoadSegment_ModifiedGeometryToLessThan70PercentOverlap()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        // extract
        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();

        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1 });
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1 });

        var laneDbaseRecord = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord });

        var widthDbaseRecord = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord });

        var surfaceDbaseRecord = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord });

        // change
        var geometry = GeometryTranslator.ToMultiLineString(roadSegmentShapeContent1.Shape);
        var lineString = (LineString)geometry[0];
        lineString = new LineString(new[]
        {
            lineString.Coordinates[0],
            new CoordinateM(lineString.Coordinates[1].X + 9000, lineString.Coordinates[1].Y, lineString.Coordinates[1].M)
        });
        geometry = new MultiLineString(new[] { lineString });
        var roadSegmentShapeContentChanged = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContentChanged });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1 });

        // assert
        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: fixture.CreateProjectionFormatFileWithOneRecord(),
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            laneExtractStream: laneExtractStream,
            widthExtractStream: widthExtractStream,
            surfaceExtractStream: surfaceExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            laneChangeStream: laneExtractStream,
            widthChangeStream: widthExtractStream,
            surfaceChangeStream: surfaceExtractStream
        );

        var expected = TranslatedChanges.Empty
            .AppendChange(new AddRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value + 1),
                    new RoadNodeId(roadSegmentDbaseRecord1.B_WK_OIDN.Value),
                    new RoadNodeId(roadSegmentDbaseRecord1.E_WK_OIDN.Value),
                    new OrganizationId(roadSegmentDbaseRecord1.BEHEER.Value),
                    RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord1.METHODE.Value],
                    RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord1.MORF.Value],
                    RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord1.STATUS.Value],
                    RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord1.WEGCAT.Value],
                    RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord1.TGBEP.Value],
                    CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.LSTRNMID.Value),
                    CrabStreetnameId.FromValue(roadSegmentDbaseRecord1.RSTRNMID.Value)
                )
                .WithGeometry(GeometryTranslator.ToMultiLineString(roadSegmentShapeContentChanged.Shape))
                .WithLane(
                    new RoadSegmentLaneAttribute(
                        new AttributeId(laneDbaseRecord.RS_OIDN.Value),
                        new RoadSegmentLaneCount(laneDbaseRecord.AANTAL.Value),
                        RoadSegmentLaneDirection.ByIdentifier[laneDbaseRecord.RICHTING.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithWidth(
                    new RoadSegmentWidthAttribute(
                        new AttributeId(widthDbaseRecord.WB_OIDN.Value),
                        new RoadSegmentWidth(widthDbaseRecord.BREEDTE.Value),
                        new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithSurface(
                    new RoadSegmentSurfaceAttribute(
                        new AttributeId(surfaceDbaseRecord.WV_OIDN.Value),
                        RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord.TYPE.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord.TOTPOS.Value))
                    )
                ))
            .AppendChange(new RemoveRoadSegment(
                new RecordNumber(1),
                new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value)
            ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task RoadSegment_ModifiedNonCriticalAttribute()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        // extract
        var roadSegmentDbaseRecord = fixture.Create<RoadSegmentDbaseRecord>();
        var roadSegmentShapeContent = fixture.Create<PolyLineMShapeContent>();

        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord });
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent });

        // change
        roadSegmentDbaseRecord.STATUS.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord.STATUS.Value]).Translation.Identifier;

        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord });

        // assert
        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentProjectionFormatStream: fixture.CreateProjectionFormatFileWithOneRecord(),
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream
        );

        var expected = TranslatedChanges.Empty
            .AppendChange(new ModifyRoadSegment(
                new RecordNumber(1),
                new RoadSegmentId(roadSegmentDbaseRecord.WS_OIDN.Value),
                new RoadNodeId(roadSegmentDbaseRecord.B_WK_OIDN.Value),
                new RoadNodeId(roadSegmentDbaseRecord.E_WK_OIDN.Value),
                new OrganizationId(roadSegmentDbaseRecord.BEHEER.Value),
                RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord.METHODE.Value],
                RoadSegmentMorphology.ByIdentifier[roadSegmentDbaseRecord.MORF.Value],
                RoadSegmentStatus.ByIdentifier[roadSegmentDbaseRecord.STATUS.Value],
                RoadSegmentCategory.ByIdentifier[roadSegmentDbaseRecord.WEGCAT.Value],
                RoadSegmentAccessRestriction.ByIdentifier[roadSegmentDbaseRecord.TGBEP.Value],
                CrabStreetnameId.FromValue(roadSegmentDbaseRecord.LSTRNMID.Value),
                CrabStreetnameId.FromValue(roadSegmentDbaseRecord.RSTRNMID.Value)
            ).WithGeometry(GeometryTranslator.ToMultiLineString(roadSegmentShapeContent.Shape)));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task TranslateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveFeatureCompareTranslator(Encoding, _logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Translate(null, CancellationToken.None));
    }

    private async Task TranslateReturnsExpectedResult(ZipArchive archive, TranslatedChanges expected)
    {
        using (archive)
        {
            var sut = new ZipArchiveFeatureCompareTranslator(Encoding, _logger);

            var result = await sut.Translate(archive, CancellationToken.None);

            Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
        }
    }
}