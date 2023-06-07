namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Extracts;

using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using Uploads;

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

        using (zipArchive)
        {
            var sut = new ZipArchiveFeatureCompareTranslator(Encoding, _logger);

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