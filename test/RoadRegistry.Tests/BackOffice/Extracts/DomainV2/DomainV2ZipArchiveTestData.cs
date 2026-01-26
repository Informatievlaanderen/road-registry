namespace RoadRegistry.Tests.BackOffice.Extracts.DomainV2;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.Extracts.Schemas.DomainV2;
using RoadRegistry.Extracts.Schemas.DomainV2.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadNodes;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class DomainV2ZipArchiveTestData : IDisposable
{
    public DomainV2ZipArchiveTestData()
    {
        Fixture = CreateFixture();

        EmptyZipArchive = CreateEmptyZipArchive();
        ZipArchiveWithEmptyFiles = CreateZipArchiveWithEmptyFiles();
        ZipArchive = CreateZipArchiveWithEachFileAtLeastOneRecord();
    }

    public ZipArchive EmptyZipArchive { get; }
    public ZipArchive ZipArchiveWithEmptyFiles { get; }
    public ZipArchive ZipArchive { get; }
    public Fixture Fixture { get; }

    public void Dispose()
    {
        ZipArchive?.Dispose();
    }

    private ZipArchive CreateEmptyZipArchive()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            archive.CreateEntry(Guid.NewGuid().ToString("N"));
        }

        stream.Position = 0;

        return new ZipArchive(stream, ZipArchiveMode.Read, false);
    }

    private static Fixture CreateFixture()
    {
        var fixture = FixtureFactory.Create();

        fixture.CustomizeRecordType();
        fixture.CustomizeAttributeId();
        fixture.CustomizeRoadSegmentId();
        fixture.CustomizeEuropeanRoadNumber();
        fixture.CustomizeNationalRoadNumber();
        fixture.CustomizeGradeSeparatedJunctionId();
        fixture.CustomizeGradeSeparatedJunctionTypeV2();
        fixture.CustomizeRoadNodeId();
        fixture.CustomizeRoadNodeTypeV2();
        fixture.CustomizeRoadSegmentGeometryDrawMethodV2();
        fixture.CustomizeOrganizationId();
        fixture.CustomizeRoadSegmentMorphologyV2();
        fixture.CustomizeRoadSegmentStatusV2();
        fixture.CustomizeRoadSegmentCategoryV2();
        fixture.CustomizeRoadSegmentAccessRestrictionV2();
        fixture.CustomizeRoadSegmentPosition();
        fixture.CustomizeRoadSegmentSurfaceTypeV2();
        fixture.CustomizeStreetNameLocalId();
        fixture.CustomizeOperatorName();
        fixture.CustomizeReason();
        fixture.CustomizeDownloadId();

        fixture.Customize<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<GradeSeparatedJunctionDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE =
                        { Value = (short)fixture.Create<GradeSeparatedJunctionTypeV2>().Translation.Identifier },
                    BO_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadSegmentNationalRoadAttributeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentNationalRoadAttributeDbaseRecord
                {
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_TEMPID = { Value = fixture.Create<RoadSegmentId>().ToInt32() },
                    NWNUMMER = { Value = fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        fixture.Customize<RoadNodeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadNodeDbaseRecord
                {
                    WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)fixture.Create<RoadNodeTypeV2>().Translation.Identifier }
                })
                .OmitAutoProperties());

        fixture.Customize<Point>(customization =>
            customization.FromFactory(generator =>
                new Point(
                    fixture.Create<double>(),
                    fixture.Create<double>()
                )
            ).OmitAutoProperties()
        );
        fixture.Customize<RoadRegistry.BackOffice.Messages.Point>(customization =>
            customization.FromFactory(generator =>
                new RoadRegistry.BackOffice.Messages.Point
                {
                    X = fixture.Create<double>(),
                    Y = fixture.Create<double>()
                }
            ).OmitAutoProperties()
        );

        fixture.Customize<RecordNumber>(customizer =>
            customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));

        fixture.Customize<PointShapeContent>(customization =>
            customization
                .FromFactory(random => new PointShapeContent(
                    GeometryTranslator.FromGeometryPoint(fixture.Create<Point>())))
                .OmitAutoProperties()
        );

        fixture.Customize<LineString>(customization =>
            customization.FromFactory(generator =>
                {
                    var x = generator.Next(15000, 99000);
                    var y = generator.Next(21000, 99000);
                    var m = generator.Next(5, 100);

                    return new LineString(
                        new CoordinateArraySequence(
                        [
                            new CoordinateM(x, y, 0),
                                new CoordinateM(x + m, y, m)
                        ]),
                        GeometryConfiguration.GeometryFactory
                    );
                }
            ).OmitAutoProperties()
        );
        fixture.Customize<Polygon>(customization =>
            customization.FromFactory(generator =>
                {
                    var x = generator.Next(15000, 99000);
                    var y = generator.Next(21000, 99000);
                    var width = generator.Next(10, 1000);

                    return new Polygon(new LinearRing([
                        new(x, y),
                        new(x, y + width),
                        new(x + width, y + width),
                        new(x + width, y),
                        new(x, y)
                    ]));
                }
            ).OmitAutoProperties());

        fixture.Customize<MultiLineString>(customization =>
            customization.FromFactory(_ =>
                new MultiLineString([fixture.Create<LineString>()])
            ).OmitAutoProperties()
        );
        fixture.Customize<PolyLineMShapeContent>(customization =>
            customization
                .FromFactory(_ => fixture.Create<LineString>().ToShapeContent())
                .OmitAutoProperties()
        );
        fixture.Customize<PolygonShapeContent>(customization =>
            customization
                .FromFactory(_ => fixture.Create<Polygon>().ToShapeContent())
                .OmitAutoProperties()
        );

        fixture.Customize<RoadSegmentDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentDbaseRecord
                {
                    WS_TEMPID = { Value = random.Next(1, int.MaxValue) },
                    WS_OIDN = { Value = random.Next(1, int.MaxValue) },
                    LBEHEER = { Value = fixture.Create<OrganizationId>() },
                    RBEHEER = { Value = fixture.Create<OrganizationId>() },
                    MORF = { Value = fixture.Create<RoadSegmentMorphologyV2>().Translation.Identifier },
                    STATUS = { Value = fixture.Create<RoadSegmentStatusV2>().Translation.Identifier },
                    WEGCAT = { Value = fixture.Create<RoadSegmentCategoryV2>().Translation.Identifier },
                    LSTRNMID = { Value = fixture.Create<StreetNameLocalId>() },
                    RSTRNMID = { Value = fixture.Create<StreetNameLocalId>() },
                    TOEGANG = { Value = fixture.Create<RoadSegmentAccessRestrictionV2>().Translation.Identifier },
                    VERHARDING = { Value = fixture.Create<RoadSegmentSurfaceTypeV2>().Translation.Identifier },
                    AUTOHEEN = { Value = random.Next(0, 2) },
                    AUTOTERUG = { Value = random.Next(0, 2) },
                    FIETSHEEN = { Value = random.Next(0, 2) },
                    FIETSTERUG = { Value = random.Next(0, 2) },
                    VOETGANGER = { Value = random.Next(0, 2) }
                })
                .OmitAutoProperties());

        fixture.Customize<TransactionZoneDbaseRecord>(
            composer => composer
                .FromFactory(random => new TransactionZoneDbaseRecord
                {
                    TYPE = { Value = random.Next(1, 9999) },
                    BESCHRIJV = { Value = fixture.Create<Reason>().ToString() },
                    DOWNLOADID = { Value = fixture.Create<DownloadId>().ToString() },
                })
                .OmitAutoProperties());
        return fixture;
    }

    private ZipArchive CreateZipArchive(
        MemoryStream roadSegmentShapeChangeStream,
        MemoryStream roadSegmentProjectionFormatStream,
        MemoryStream roadSegmentDbaseChangeStream,
        MemoryStream roadSegmentShapeIntegrationStream,
        MemoryStream roadSegmentDbaseIntegrationStream,
        MemoryStream roadNodeShapeChangeStream,
        MemoryStream roadNodeProjectionFormatStream,
        MemoryStream roadNodeDbaseChangeStream,
        MemoryStream roadNodeShapeIntegrationStream,
        MemoryStream roadNodeDbaseIntegrationStream,
        MemoryStream europeanRoadChangeStream,
        MemoryStream nationalRoadChangeStream,
        MemoryStream gradeSeparatedJunctionChangeStream,
        MemoryStream transactionZoneShapeStream,
        MemoryStream transactionZoneProjectionFormatStream,
        MemoryStream transactionZoneDbaseStream
    )
    {
        var requiredFiles = new[]
        {
            "TRANSACTIEZONES.SHP",
            "TRANSACTIEZONES.PRJ",
            "TRANSACTIEZONES.DBF",
            "IWEGKNOOP.DBF",
            "EWEGKNOOP.DBF",
            "WEGKNOOP.DBF",
            "IWEGKNOOP.SHP",
            "EWEGKNOOP.SHP",
            "WEGKNOOP.SHP",
            "IWEGKNOOP.PRJ",
            "EWEGKNOOP.PRJ",
            "WEGKNOOP.PRJ",
            "IWEGSEGMENT.DBF",
            "EWEGSEGMENT.DBF",
            "WEGSEGMENT.DBF",
            "IWEGSEGMENT.SHP",
            "EWEGSEGMENT.SHP",
            "WEGSEGMENT.SHP",
            "IWEGSEGMENT.PRJ",
            "EWEGSEGMENT.PRJ",
            "WEGSEGMENT.PRJ",
            "EATTWEGVERHARDING.DBF",
            "ATTWEGVERHARDING.DBF",
            "EATTEUROPWEG.DBF",
            "ATTEUROPWEG.DBF",
            "EATTNATIONWEG.DBF",
            "ATTNATIONWEG.DBF",
            "ERLTOGKRUISING.DBF",
            "RLTOGKRUISING.DBF"
        };

        var archiveStream = new MemoryStream();
        using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            void CreateEntry(string file, MemoryStream fileStream)
            {
                using (var entryStream = createArchive.CreateEntry(file).Open())
                {
                    fileStream.Position = 0;
                    fileStream.CopyTo(entryStream);
                }
            }

            foreach (var requiredFile in requiredFiles)
            {
                switch (requiredFile)
                {
                    case "EWEGSEGMENT.SHP":
                    case "WEGSEGMENT.SHP":
                        CreateEntry(requiredFile, roadSegmentShapeChangeStream);
                        break;
                    case "IWEGSEGMENT.SHP":
                        CreateEntry(requiredFile, roadSegmentShapeIntegrationStream);
                        break;
                    case "IWEGSEGMENT.PRJ":
                    case "EWEGSEGMENT.PRJ":
                    case "WEGSEGMENT.PRJ":
                        CreateEntry(requiredFile, roadSegmentProjectionFormatStream);
                        break;
                    case "EWEGSEGMENT.DBF":
                    case "WEGSEGMENT.DBF":
                        CreateEntry(requiredFile, roadSegmentDbaseChangeStream);
                        break;
                    case "IWEGSEGMENT.DBF":
                        CreateEntry(requiredFile, roadSegmentDbaseIntegrationStream);
                        break;
                    case "EWEGKNOOP.SHP":
                    case "WEGKNOOP.SHP":
                        CreateEntry(requiredFile, roadNodeShapeChangeStream);
                        break;
                    case "IWEGKNOOP.SHP":
                        CreateEntry(requiredFile, roadNodeShapeIntegrationStream);
                        break;
                    case "IWEGKNOOP.PRJ":
                    case "EWEGKNOOP.PRJ":
                    case "WEGKNOOP.PRJ":
                        CreateEntry(requiredFile, roadNodeProjectionFormatStream);
                        break;
                    case "EWEGKNOOP.DBF":
                    case "WEGKNOOP.DBF":
                        CreateEntry(requiredFile, roadNodeDbaseChangeStream);
                        break;
                    case "IWEGKNOOP.DBF":
                        CreateEntry(requiredFile, roadNodeDbaseIntegrationStream);
                        break;
                    case "EATTEUROPWEG.DBF":
                    case "ATTEUROPWEG.DBF":
                        CreateEntry(requiredFile, europeanRoadChangeStream);
                        break;
                    case "EATTNATIONWEG.DBF":
                    case "ATTNATIONWEG.DBF":
                        CreateEntry(requiredFile, nationalRoadChangeStream);
                        break;
                    case "ERLTOGKRUISING.DBF":
                    case "RLTOGKRUISING.DBF":
                        CreateEntry(requiredFile, gradeSeparatedJunctionChangeStream);
                        break;
                    case "TRANSACTIEZONES.SHP":
                        CreateEntry(requiredFile, transactionZoneShapeStream);
                        break;
                    case "TRANSACTIEZONES.PRJ":
                        CreateEntry(requiredFile, transactionZoneProjectionFormatStream);
                        break;
                    case "TRANSACTIEZONES.DBF":
                        CreateEntry(requiredFile, transactionZoneDbaseStream);
                        break;
                }
            }
        }

        archiveStream.Position = 0;

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
    }

    private ZipArchive CreateZipArchiveWithEmptyFiles()
    {
        var roadSegmentShapeChangeStream = Fixture.CreateEmptyRoadSegmentShapeFile();
        var roadSegmentProjectionFormatStream = Fixture.CreateEmptyProjectionFormatFile();
        var roadSegmentDbaseChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema);

        var europeanRoadChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
        var nationalRoadChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(RoadSegmentNationalRoadAttributeDbaseRecord.Schema);

        var roadNodeShapeChangeStream = Fixture.CreateEmptyRoadNodeShapeFile();
        var roadNodeProjectionFormatStream = Fixture.CreateEmptyProjectionFormatFile();
        var roadNodeDbaseChangeStream = Fixture.CreateEmptyDbfFile<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema);

        var gradeSeparatedJunctionChangeStream = Fixture.CreateEmptyDbfFile<GradeSeparatedJunctionDbaseRecord>(GradeSeparatedJunctionDbaseRecord.Schema);

        var transactionZoneShapeStream = Fixture.CreateEmptyRoadSegmentShapeFile();
        var transactionZoneProjectionFormatStream = Fixture.CreateEmptyProjectionFormatFile();
        var transactionZoneDbaseStream = Fixture.CreateEmptyDbfFile<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

        return CreateZipArchive(
            roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream,
            roadSegmentDbaseChangeStream,
            roadSegmentShapeChangeStream,
            roadSegmentDbaseChangeStream,
            roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream,
            roadNodeDbaseChangeStream,
            roadNodeShapeChangeStream,
            roadNodeDbaseChangeStream,
            europeanRoadChangeStream,
            nationalRoadChangeStream,
            gradeSeparatedJunctionChangeStream,
            transactionZoneShapeStream,
            transactionZoneProjectionFormatStream,
            transactionZoneDbaseStream
        );
    }

    private ZipArchive CreateZipArchiveWithEachFileAtLeastOneRecord()
    {
        var roadNodeProjectionFormatStream = Fixture.CreateProjectionFormatFileWithOneRecord();
        var roadNodeShapeChangeStream = Fixture.CreateRoadNodeShapeFile([
            Fixture.Create<PointShapeContent>(),
            Fixture.Create<PointShapeContent>()
        ]);
        var roadNodeShapeIntegrationStream = Fixture.CreateRoadNodeShapeFile([
            Fixture.Create<PointShapeContent>(),
            Fixture.Create<PointShapeContent>()
        ]);
        var roadNodeDbaseChange1 = Fixture.Create<RoadNodeDbaseRecord>();
        var roadNodeDbaseChange2 = Fixture.Create<RoadNodeDbaseRecord>();
        var roadNodeDbaseChangeStream = Fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, [roadNodeDbaseChange1, roadNodeDbaseChange2]);

        var roadNodeDbaseIntegration1 = Fixture.CreateWhichIsDifferentThan<RoadNodeDbaseRecord>((x1, x2) => x1.WK_OIDN.Value == x2.WK_OIDN.Value);
        var roadNodeDbaseIntegration2 = Fixture.CreateWhichIsDifferentThan<RoadNodeDbaseRecord>((x1, x2) => x1.WK_OIDN.Value == x2.WK_OIDN.Value);
        var roadNodeDbaseIntegrationStream = Fixture.CreateDbfFile(RoadNodeDbaseRecord.Schema, [roadNodeDbaseIntegration1, roadNodeDbaseIntegration2]);

        var roadSegmentPolyLineMShapeContent = Fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeChangeStream = Fixture.CreateRoadSegmentShapeFileWithOneRecord(roadSegmentPolyLineMShapeContent);
        var roadSegmentShapeIntegrationStream = Fixture.CreateRoadSegmentShapeFileWithOneRecord(roadSegmentPolyLineMShapeContent);
        var roadSegmentProjectionFormatStream = Fixture.CreateProjectionFormatFileWithOneRecord();
        var roadSegmentChangeDbaseRecord = Fixture.Create<RoadSegmentDbaseRecord>();
        //roadSegmentChangeDbaseRecord.B_WK_OIDN.Value = roadNodeDbaseChange1.WK_OIDN.Value;
        //roadSegmentChangeDbaseRecord.E_WK_OIDN.Value = roadNodeDbaseChange2.WK_OIDN.Value;
        var roadSegmentDbaseChangeStream = Fixture.CreateDbfFileWithOneRecord(RoadSegmentDbaseRecord.Schema, roadSegmentChangeDbaseRecord);
        var roadSegmentIntegrationDbaseRecord = Fixture.CreateWhichIsDifferentThan<RoadSegmentDbaseRecord>((x1, x2) => x1.WS_OIDN.Value == x2.WS_OIDN.Value);
        //roadSegmentIntegrationDbaseRecord.B_WK_OIDN.Value = roadNodeDbaseIntegration1.WK_OIDN.Value;
        //roadSegmentIntegrationDbaseRecord.E_WK_OIDN.Value = roadNodeDbaseIntegration2.WK_OIDN.Value;
        var roadSegmentDbaseIntegrationStream = Fixture.CreateDbfFileWithOneRecord(RoadSegmentDbaseRecord.Schema, roadSegmentIntegrationDbaseRecord);

        var europeanRoadChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
            RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
        var nationalRoadChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentNationalRoadAttributeDbaseRecord>(
            RoadSegmentNationalRoadAttributeDbaseRecord.Schema);

        var gradeSeparatedJunctionDbaseRecord = Fixture.Create<GradeSeparatedJunctionDbaseRecord>();
        gradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = roadSegmentChangeDbaseRecord.WS_TEMPID.Value;
        gradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value = roadSegmentChangeDbaseRecord.WS_TEMPID.Value;
        var gradeSeparatedJunctionChangeStream = Fixture.CreateDbfFileWithOneRecord(GradeSeparatedJunctionDbaseRecord.Schema, gradeSeparatedJunctionDbaseRecord);

        var transactionZoneProjectionFormatStream = Fixture.CreateProjectionFormatFileWithOneRecord();
        var transactionZoneShapeStream = Fixture.CreateTransactionZoneShapeFile([
            Fixture.Create<PolygonShapeContent>()
        ]);
        var transactionZoneDbaseStream = Fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

        return CreateZipArchive(
            roadSegmentShapeChangeStream,
            roadSegmentProjectionFormatStream,
            roadSegmentDbaseChangeStream,
            roadSegmentShapeIntegrationStream,
            roadSegmentDbaseIntegrationStream,
            roadNodeShapeChangeStream,
            roadNodeProjectionFormatStream,
            roadNodeDbaseChangeStream,
            roadNodeShapeIntegrationStream,
            roadNodeDbaseIntegrationStream,
            europeanRoadChangeStream,
            nationalRoadChangeStream,
            gradeSeparatedJunctionChangeStream,
            transactionZoneShapeStream,
            transactionZoneProjectionFormatStream,
            transactionZoneDbaseStream
        );
    }
}
