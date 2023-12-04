namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.MultipleSchemasSupport.UploadsV2;

using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.V2.Schema;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;

public class FeaturesReaderTests
{
    private static readonly Encoding Encoding = Encoding.UTF8;
    private readonly ILogger<ZipArchiveFeatureCompareTranslator> _logger;

    public FeaturesReaderTests(ILogger<ZipArchiveFeatureCompareTranslator> logger)
    {
        _logger = logger;
    }

    [Fact]
    public async Task AllFeatureReadersCanRead()
    {
        var testData = new ExtractsZipArchiveTestData();
        var fixture = CreateFixture(testData);

        var roadSegmentProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadSegmentDbaseRecord1 = fixture.Create<RoadSegmentDbaseRecord>();
        roadSegmentDbaseRecord1.WS_OIDN.Value = 1;
        roadSegmentDbaseRecord1.B_WK_OIDN.Value = 1;
        roadSegmentDbaseRecord1.E_WK_OIDN.Value = 2;
        var roadSegmentDbaseRecord2 = fixture.Create<RoadSegmentDbaseRecord>();
        roadSegmentDbaseRecord2.WS_OIDN.Value = 2;
        roadSegmentDbaseRecord2.B_WK_OIDN.Value = 3;
        roadSegmentDbaseRecord2.E_WK_OIDN.Value = 4;
        var roadSegmentDbaseExtractStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1 });
        var roadSegmentDbaseChangeStream = fixture.CreateDbfFile(RoadSegmentDbaseRecord.Schema, new[] { roadSegmentDbaseRecord1, roadSegmentDbaseRecord2 });

        var roadSegmentShapeContent1 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeContent2 = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeExtractStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1 });
        var roadSegmentShapeChangeStream = fixture.CreateRoadSegmentShapeFile(new[] { roadSegmentShapeContent1, roadSegmentShapeContent2 });

        var europeanRoadDbaseRecord = fixture.Create<RoadSegmentEuropeanRoadAttributeDbaseRecord>();
        europeanRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var europeanRoadChangeStream = fixture.CreateDbfFile(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, new[] { europeanRoadDbaseRecord });

        var nationalRoadDbaseRecord = fixture.Create<RoadSegmentNationalRoadAttributeDbaseRecord>();
        nationalRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var nationalRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNationalRoadAttributeDbaseRecord.Schema, new[] { nationalRoadDbaseRecord });

        var numberedRoadDbaseRecord = fixture.Create<RoadSegmentNumberedRoadAttributeDbaseRecord>();
        numberedRoadDbaseRecord.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var numberedRoadChangeStream = fixture.CreateDbfFile(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, new[] { numberedRoadDbaseRecord });

        var laneDbaseRecord1 = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord1.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        laneDbaseRecord1.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        laneDbaseRecord1.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var laneDbaseRecord2 = fixture.Create<RoadSegmentLaneAttributeDbaseRecord>();
        laneDbaseRecord2.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        laneDbaseRecord2.VANPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Min;
        laneDbaseRecord2.TOTPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Max;
        var laneExtractStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord1 });
        var laneChangeStream = fixture.CreateDbfFile(RoadSegmentLaneAttributeDbaseRecord.Schema, new[] { laneDbaseRecord1, laneDbaseRecord2 });

        var widthDbaseRecord1 = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord1.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        widthDbaseRecord1.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        widthDbaseRecord1.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var widthDbaseRecord2 = fixture.Create<RoadSegmentWidthAttributeDbaseRecord>();
        widthDbaseRecord2.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        widthDbaseRecord2.VANPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Min;
        widthDbaseRecord2.TOTPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Max;
        var widthExtractStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord1 });
        var widthChangeStream = fixture.CreateDbfFile(RoadSegmentWidthAttributeDbaseRecord.Schema, new[] { widthDbaseRecord1, widthDbaseRecord2 });

        var surfaceDbaseRecord1 = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord1.WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        surfaceDbaseRecord1.VANPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Min;
        surfaceDbaseRecord1.TOTPOS.Value = roadSegmentShapeContent1.Shape.MeasureRange.Max;
        var surfaceDbaseRecord2 = fixture.Create<RoadSegmentSurfaceAttributeDbaseRecord>();
        surfaceDbaseRecord2.WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        surfaceDbaseRecord2.VANPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Min;
        surfaceDbaseRecord2.TOTPOS.Value = roadSegmentShapeContent2.Shape.MeasureRange.Max;
        var surfaceExtractStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord1 });
        var surfaceChangeStream = fixture.CreateDbfFile(RoadSegmentSurfaceAttributeDbaseRecord.Schema, new[] { surfaceDbaseRecord1, surfaceDbaseRecord2 });

        var roadNodeProjectionFormatStream = fixture.CreateProjectionFormatFileWithOneRecord();

        var roadNodeDbaseRecord1 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord1.WK_OIDN.Value = roadSegmentDbaseRecord1.B_WK_OIDN.Value;
        var roadNodeDbaseRecord2 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord2.WK_OIDN.Value = roadSegmentDbaseRecord1.E_WK_OIDN.Value;
        var roadNodeDbaseRecord3 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord3.WK_OIDN.Value = roadSegmentDbaseRecord2.B_WK_OIDN.Value;
        var roadNodeDbaseRecord4 = fixture.Create<RoadNodeDbaseRecord>();
        roadNodeDbaseRecord4.WK_OIDN.Value = roadSegmentDbaseRecord2.E_WK_OIDN.Value;
        var roadNodeDbaseExtractStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2 });
        var roadNodeDbaseChangeStream = fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, new[] { roadNodeDbaseRecord1, roadNodeDbaseRecord2, roadNodeDbaseRecord3, roadNodeDbaseRecord4 });

        var roadNodeShapeContent1 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent2 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent3 = fixture.Create<PointShapeContent>();
        var roadNodeShapeContent4 = fixture.Create<PointShapeContent>();
        var roadNodeShapeExtractStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2 });
        var roadNodeShapeChangeStream = fixture.CreateRoadNodeShapeFile(new[] { roadNodeShapeContent1, roadNodeShapeContent2, roadNodeShapeContent3, roadNodeShapeContent4 });

        var gradeSeparatedJunctionDbaseRecord = fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value = roadSegmentDbaseRecord1.WS_OIDN.Value;
        gradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value = roadSegmentDbaseRecord2.WS_OIDN.Value;
        var gradeSeparatedJunctionChangeStream = fixture.CreateDbfFile(GradeSeparatedJunctionDbaseRecord.Schema, new[] { gradeSeparatedJunctionDbaseRecord });

        var zipArchive = fixture.CreateUploadZipArchive(testData,
            roadSegmentShapeExtractStream: roadSegmentShapeExtractStream,
            roadSegmentShapeChangeStream: roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream: roadSegmentProjectionFormatStream,
            roadSegmentDbaseExtractStream: roadSegmentDbaseExtractStream,
            roadSegmentDbaseChangeStream: roadSegmentDbaseChangeStream,
            roadNodeShapeExtractStream: roadNodeShapeExtractStream,
            roadNodeShapeChangeStream: roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream: roadNodeProjectionFormatStream,
            roadNodeDbaseExtractStream: roadNodeDbaseExtractStream,
            roadNodeDbaseChangeStream: roadNodeDbaseChangeStream,
            europeanRoadChangeStream: europeanRoadChangeStream,
            numberedRoadChangeStream: numberedRoadChangeStream,
            nationalRoadChangeStream: nationalRoadChangeStream,
            laneExtractStream: laneExtractStream,
            laneChangeStream: laneChangeStream,
            widthExtractStream: widthExtractStream,
            widthChangeStream: widthChangeStream,
            surfaceExtractStream: surfaceExtractStream,
            surfaceChangeStream: surfaceChangeStream,
            gradeSeparatedJunctionChangeStream: gradeSeparatedJunctionChangeStream
        );

        var maxRoadSegmentId = Math.Max(roadSegmentDbaseRecord1.WS_OIDN.Value, roadSegmentDbaseRecord2.WS_OIDN.Value);
        var roadSegment2TemporaryId = new RoadSegmentId(maxRoadSegmentId + 1);

        var expected = TranslatedChanges.Empty
            .AppendChange(
                new AddRoadNode(
                    new RecordNumber(3),
                    new RoadNodeId(roadNodeDbaseRecord3.WK_OIDN.Value),
                    new RoadNodeId(roadNodeDbaseRecord3.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord3.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent3.Shape))
            )
            .AppendChange(
                new AddRoadNode(
                    new RecordNumber(4),
                    new RoadNodeId(roadNodeDbaseRecord4.WK_OIDN.Value),
                    new RoadNodeId(roadNodeDbaseRecord4.WK_OIDN.Value),
                    RoadNodeType.ByIdentifier[roadNodeDbaseRecord4.TYPE.Value]
                ).WithGeometry(GeometryTranslator.ToPoint(roadNodeShapeContent4.Shape))
            )
            .AppendChange(
                new AddRoadSegment(
                    new RecordNumber(2),
                    roadSegment2TemporaryId,
                    new RoadSegmentId(roadSegmentDbaseRecord2.WS_OIDN.Value),
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
                        .WithLane(
                            new RoadSegmentLaneAttribute(
                                new AttributeId(laneDbaseRecord2.RS_OIDN.Value),
                                new RoadSegmentLaneCount(laneDbaseRecord2.AANTAL.Value),
                                RoadSegmentLaneDirection.ByIdentifier[laneDbaseRecord2.RICHTING.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord2.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(laneDbaseRecord2.TOTPOS.Value))
                            )
                        )
                        .WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(widthDbaseRecord2.WB_OIDN.Value),
                                new RoadSegmentWidth(widthDbaseRecord2.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord2.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(widthDbaseRecord2.TOTPOS.Value))
                            )
                        )
                        .WithSurface(
                            new RoadSegmentSurfaceAttribute(
                                new AttributeId(surfaceDbaseRecord2.WV_OIDN.Value),
                                RoadSegmentSurfaceType.ByIdentifier[surfaceDbaseRecord2.TYPE.Value],
                                new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord2.VANPOS.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(surfaceDbaseRecord2.TOTPOS.Value))
                            )
                        )
            )
            .AppendChange(
                new AddRoadSegmentToEuropeanRoad
                (
                    new RecordNumber(1),
                    new AttributeId(europeanRoadDbaseRecord.EU_OIDN.Value),
                    roadSegment2TemporaryId,
                    EuropeanRoadNumber.Parse(europeanRoadDbaseRecord.EUNUMMER.Value)
                )
            )
            .AppendChange(
                new AddRoadSegmentToNationalRoad
                (
                    new RecordNumber(1),
                    new AttributeId(nationalRoadDbaseRecord.NW_OIDN.Value),
                    roadSegment2TemporaryId,
                    NationalRoadNumber.Parse(nationalRoadDbaseRecord.IDENT2.Value)
                )
            )
            .AppendChange(
                new AddRoadSegmentToNumberedRoad
                (
                    new RecordNumber(1),
                    new AttributeId(numberedRoadDbaseRecord.GW_OIDN.Value),
                    roadSegment2TemporaryId,
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
                    new RoadSegmentId(roadSegmentDbaseRecord1.WS_OIDN.Value),
                    roadSegment2TemporaryId
                )
            );

        using (zipArchive)
        {
            var sut = new ZipArchiveFeatureCompareTranslator(Encoding, _logger, new UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle(true));

            var result = await sut.Translate(zipArchive, CancellationToken.None);

            Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
        }
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
}
