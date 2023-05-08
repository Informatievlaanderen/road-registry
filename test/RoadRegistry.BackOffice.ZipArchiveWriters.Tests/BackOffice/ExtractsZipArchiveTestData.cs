namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using RoadRegistry.BackOffice.Extracts.Dbase;
    using RoadRegistry.BackOffice.Extracts.Dbase.GradeSeparatedJuntions;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.Tests.BackOffice;
    using System.IO.Compression;
    using System.Text;
    using Uploads;
    using Validation;
    using Point = NetTopologySuite.Geometries.Point;

    public class ExtractsZipArchiveTestData : IDisposable
    {
        public ZipArchive EmptyZipArchive { get; }
        public ZipArchive ZipArchive { get; }
        public ZipArchive ZipArchiveWithEmptyFiles { get; }
        public Fixture Fixture { get; }

        public readonly string[] ExtractFileNames = {
            "EWEGKNOOP.DBF",
            "EWEGKNOOP.SHP",
            "EWEGSEGMENT.DBF",
            "EWEGSEGMENT.SHP",
            "EATTRIJSTROKEN.DBF",
            "EATTWEGBREEDTE.DBF",
            "EATTWEGVERHARDING.DBF",
            "EATTEUROPWEG.DBF",
            "EATTNATIONWEG.DBF",
            "EATTGENUMWEG.DBF",
            "ERLTOGKRUISING.DBF"
        };

        public ExtractsZipArchiveTestData()
        {
            Fixture = CreateFixture();

            EmptyZipArchive = CreateEmptyZipArchive();
            ZipArchive = CreateZipArchiveWithOneRecord();
            ZipArchiveWithEmptyFiles = CreateZipArchiveWithEmptyFiles();
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

        private ZipArchive CreateZipArchiveWithOneRecord()
        {
            var roadSegmentPolyLineMShapeContent = Fixture.Create<PolyLineMShapeContent>();
            var roadSegmentShapeChangeStream = Fixture.CreateRoadSegmentShapeFileWithOneRecord(roadSegmentPolyLineMShapeContent);
            var roadSegmentProjectionFormatStream = Fixture.CreateProjectionFormatFileWithOneRecord();
            var roadSegmentChangeDbaseRecord = Fixture.Create<RoadSegmentDbaseRecord>();
            var roadSegmentDbaseChangeStream = Fixture.CreateDbfFileWithOneRecord(RoadSegmentDbaseRecord.Schema, roadSegmentChangeDbaseRecord);

            var europeanRoadChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
            var nationalRoadChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentNationalRoadAttributeDbaseRecord>(
                RoadSegmentNationalRoadAttributeDbaseRecord.Schema);
            var numberedRoadChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                RoadSegmentNumberedRoadAttributeDbaseRecord.Schema);
            var laneChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentLaneAttributeDbaseRecord>(
                RoadSegmentLaneAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                    record.VANPOS.Value = roadSegmentPolyLineMShapeContent.Shape.MeasureRange.Min;
                    record.TOTPOS.Value = roadSegmentPolyLineMShapeContent.Shape.MeasureRange.Max;
                });
            var widthChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentWidthAttributeDbaseRecord>(
                RoadSegmentWidthAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                    record.VANPOS.Value = roadSegmentPolyLineMShapeContent.Shape.MeasureRange.Min;
                    record.TOTPOS.Value = roadSegmentPolyLineMShapeContent.Shape.MeasureRange.Max;
                });
            var surfaceChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadSegmentSurfaceAttributeDbaseRecord>(
                RoadSegmentSurfaceAttributeDbaseRecord.Schema,
                record =>
                {
                    record.WS_OIDN.Value = roadSegmentChangeDbaseRecord.WS_OIDN.Value;
                    record.VANPOS.Value = roadSegmentPolyLineMShapeContent.Shape.MeasureRange.Min;
                    record.TOTPOS.Value = roadSegmentPolyLineMShapeContent.Shape.MeasureRange.Max;
                });

            var roadNodeShapeChangeStream = Fixture.CreateRoadNodeShapeFileWithOneRecord();
            var roadNodeProjectionFormatStream = Fixture.CreateProjectionFormatFileWithOneRecord();
            var roadNodeDbaseChangeStream = Fixture.CreateDbfFileWithOneRecord<RoadNodeDbaseRecord>(
                RoadNodeDbaseRecord.Schema);

            var gradeSeparatedJunctionChangeStream = Fixture.CreateDbfFileWithOneRecord<GradeSeparatedJunctionDbaseRecord>(
                GradeSeparatedJunctionDbaseRecord.Schema);

            var transactionZoneStream = Fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

            return CreateZipArchive(
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
                gradeSeparatedJunctionChangeStream,
                transactionZoneStream
            );
        }
        private ZipArchive CreateZipArchiveWithEmptyFiles()
        {
            var roadSegmentShapeChangeStream = Fixture.CreateEmptyRoadSegmentShapeFile();
            var roadSegmentProjectionFormatStream = Fixture.CreateEmptyProjectionFormatFile();
            var roadSegmentDbaseChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema);

            var europeanRoadChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema);
            var nationalRoadChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(RoadSegmentNationalRoadAttributeDbaseRecord.Schema);
            var numberedRoadChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(RoadSegmentNumberedRoadAttributeDbaseRecord.Schema);
            var laneChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentLaneAttributeDbaseRecord>(RoadSegmentLaneAttributeDbaseRecord.Schema);
            var widthChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentWidthAttributeDbaseRecord>(RoadSegmentWidthAttributeDbaseRecord.Schema);
            var surfaceChangeStream = Fixture.CreateEmptyDbfFile<RoadSegmentSurfaceAttributeDbaseRecord>(RoadSegmentSurfaceAttributeDbaseRecord.Schema);

            var roadNodeShapeChangeStream = Fixture.CreateEmptyRoadNodeShapeFile();
            var roadNodeProjectionFormatStream = Fixture.CreateEmptyProjectionFormatFile();
            var roadNodeDbaseChangeStream = Fixture.CreateEmptyDbfFile<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema);

            var gradeSeparatedJunctionChangeStream = Fixture.CreateEmptyDbfFile<GradeSeparatedJunctionDbaseRecord>(GradeSeparatedJunctionDbaseRecord.Schema);

            var transactionZoneStream = Fixture.CreateEmptyDbfFile<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);

            return CreateZipArchive(
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
                gradeSeparatedJunctionChangeStream,
                transactionZoneStream
            );
        }

        private ZipArchive CreateZipArchive(
            MemoryStream roadSegmentShapeChangeStream,
            MemoryStream roadSegmentProjectionFormatStream,
            MemoryStream roadSegmentDbaseChangeStream,
            MemoryStream roadNodeShapeChangeStream,
            MemoryStream roadNodeProjectionFormatStream,
            MemoryStream roadNodeDbaseChangeStream,
            MemoryStream europeanRoadChangeStream,
            MemoryStream numberedRoadChangeStream,
            MemoryStream nationalRoadChangeStream,
            MemoryStream laneChangeStream,
            MemoryStream widthChangeStream,
            MemoryStream surfaceChangeStream,
            MemoryStream gradeSeparatedJunctionChangeStream,
            MemoryStream transactionZoneStream
        )
        {
            roadSegmentShapeChangeStream.Position = 0;
            roadSegmentProjectionFormatStream.Position = 0;
            roadSegmentDbaseChangeStream.Position = 0;
            roadNodeShapeChangeStream.Position = 0;
            roadNodeProjectionFormatStream.Position = 0;
            roadNodeDbaseChangeStream.Position = 0;
            europeanRoadChangeStream.Position = 0;
            numberedRoadChangeStream.Position = 0;
            nationalRoadChangeStream.Position = 0;
            laneChangeStream.Position = 0;
            widthChangeStream.Position = 0;
            surfaceChangeStream.Position = 0;
            gradeSeparatedJunctionChangeStream.Position = 0;
            transactionZoneStream.Position = 0;

            var requiredFiles = new[]
            {
                "TRANSACTIEZONES.DBF",
                "EWEGKNOOP.DBF",
                "WEGKNOOP.DBF",
                "EWEGKNOOP.SHP",
                "WEGKNOOP.SHP",
                "WEGKNOOP.PRJ",
                "EWEGSEGMENT.DBF",
                "WEGSEGMENT.DBF",
                "EWEGSEGMENT.SHP",
                "WEGSEGMENT.SHP",
                "WEGSEGMENT.PRJ",
                "EATTRIJSTROKEN.DBF",
                "ATTRIJSTROKEN.DBF",
                "EATTWEGBREEDTE.DBF",
                "ATTWEGBREEDTE.DBF",
                "EATTWEGVERHARDING.DBF",
                "ATTWEGVERHARDING.DBF",
                "EATTEUROPWEG.DBF",
                "ATTEUROPWEG.DBF",
                "EATTNATIONWEG.DBF",
                "ATTNATIONWEG.DBF",
                "EATTGENUMWEG.DBF",
                "ATTGENUMWEG.DBF",
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
                        case "WEGSEGMENT.PRJ":
                            CreateEntry(requiredFile, roadSegmentProjectionFormatStream);
                            break;
                        case "EWEGSEGMENT.DBF":
                        case "WEGSEGMENT.DBF":
                            CreateEntry(requiredFile, roadSegmentDbaseChangeStream);
                            break;
                        case "EWEGKNOOP.SHP":
                        case "WEGKNOOP.SHP":
                            CreateEntry(requiredFile, roadNodeShapeChangeStream);
                            break;
                        case "WEGKNOOP.PRJ":
                            CreateEntry(requiredFile, roadNodeProjectionFormatStream);
                            break;
                        case "EWEGKNOOP.DBF":
                        case "WEGKNOOP.DBF":
                            CreateEntry(requiredFile, roadNodeDbaseChangeStream);
                            break;
                        case "EATTEUROPWEG.DBF":
                        case "ATTEUROPWEG.DBF":
                            CreateEntry(requiredFile, europeanRoadChangeStream);
                            break;
                        case "EATTGENUMWEG.DBF":
                        case "ATTGENUMWEG.DBF":
                            CreateEntry(requiredFile, numberedRoadChangeStream);
                            break;
                        case "EATTNATIONWEG.DBF":
                        case "ATTNATIONWEG.DBF":
                            CreateEntry(requiredFile, nationalRoadChangeStream);
                            break;
                        case "EATTRIJSTROKEN.DBF":
                        case "ATTRIJSTROKEN.DBF":
                            CreateEntry(requiredFile, laneChangeStream);
                            break;
                        case "EATTWEGBREEDTE.DBF":
                        case "ATTWEGBREEDTE.DBF":
                            CreateEntry(requiredFile, widthChangeStream);
                            break;
                        case "EATTWEGVERHARDING.DBF":
                        case "ATTWEGVERHARDING.DBF":
                            CreateEntry(requiredFile, surfaceChangeStream);
                            break;
                        case "ERLTOGKRUISING.DBF":
                        case "RLTOGKRUISING.DBF":
                            CreateEntry(requiredFile, gradeSeparatedJunctionChangeStream);
                            break;
                        case "TRANSACTIEZONES.DBF":
                            CreateEntry(requiredFile, transactionZoneStream);
                            break;
                    }
                }
            }

            archiveStream.Position = 0;

            return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
        }

        private static Fixture CreateFixture()
        {
            var fixture = new Fixture();

            fixture.CustomizeRecordType();
            fixture.CustomizeAttributeId();
            fixture.CustomizeRoadSegmentId();
            fixture.CustomizeEuropeanRoadNumber();
            fixture.CustomizeNationalRoadNumber();
            fixture.CustomizeGradeSeparatedJunctionId();
            fixture.CustomizeGradeSeparatedJunctionType();
            fixture.CustomizeNumberedRoadNumber();
            fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            fixture.CustomizeRoadSegmentNumberedRoadDirection();
            fixture.CustomizeRoadNodeId();
            fixture.CustomizeRoadNodeType();
            fixture.CustomizeRoadSegmentGeometryDrawMethod();
            fixture.CustomizeOrganizationId();
            fixture.CustomizeRoadSegmentMorphology();
            fixture.CustomizeRoadSegmentStatus();
            fixture.CustomizeRoadSegmentCategory();
            fixture.CustomizeRoadSegmentAccessRestriction();
            fixture.CustomizeRoadSegmentLaneCount();
            fixture.CustomizeRoadSegmentLaneDirection();
            fixture.CustomizeRoadSegmentPosition();
            fixture.CustomizeRoadSegmentSurfaceType();
            fixture.CustomizeRoadSegmentPosition();
            fixture.CustomizeRoadSegmentWidth();
            fixture.CustomizeRoadSegmentPosition();
            fixture.CustomizeOrganizationId();
            fixture.CustomizeOperatorName();
            fixture.CustomizeReason();
            fixture.CustomizeDownloadId();

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
                        WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                        TYPE = { Value = (short)fixture.Create<RoadNodeType>().Translation.Identifier }
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
                        var x = generator.Next(5, 100);

                        return new LineString(
                            new CoordinateArraySequence(
                                new Coordinate[]
                                {
                                    new CoordinateM(0.0, 0.0, 0),
                                    new CoordinateM(x, 0.0, x)
                                }),
                            GeometryConfiguration.GeometryFactory
                        );
                    }
                ).OmitAutoProperties()
            );

            fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(_ =>
                    new MultiLineString(new[] { fixture.Create<LineString>() })
                ).OmitAutoProperties()
            );
            fixture.Customize<PolyLineMShapeContent>(customization =>
                customization.FromFactory(_ => new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(fixture.Create<MultiLineString>()))
                )
                .OmitAutoProperties()
            );

            fixture.Customize<RoadSegmentDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                        METHODE =
                        {
                            Value = (short)fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier
                        },
                        BEHEER = { Value = fixture.Create<OrganizationId>() },
                        MORF = { Value = (short)fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                        STATUS = { Value = fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                        WEGCAT = { Value = fixture.Create<RoadSegmentCategory>().Translation.Identifier },
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

            fixture.Customize<TransactionZoneDbaseRecord>(
                composer => composer
                    .FromFactory(random => new TransactionZoneDbaseRecord
                    {
                        SOURCEID = { Value = random.Next(1, 5) },
                        TYPE = { Value = random.Next(1, 9999) },
                        BESCHRIJV = { Value = fixture.Create<Reason>().ToString() },
                        DOWNLOADID = { Value = fixture.Create<DownloadId>().ToString() },
                        OPERATOR = { Value = fixture.Create<OperatorName>().ToString() },
                        ORG = { Value = fixture.Create<OrganizationId>().ToString() },
                        APPLICATIE =
                        {
                            Value = new string(fixture
                                .CreateMany<char>(TransactionZoneDbaseRecord.Schema.APPLICATIE.Length.ToInt32())
                                .ToArray())
                        }
                    })
                    .OmitAutoProperties());
            return fixture;
        }

        public void Dispose()
        {
            ZipArchive?.Dispose();
        }

        [Fact]
        public void ExtractsZipArchiveTestDataIsValid()
        {
            var sut = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);
            var result = sut.Validate(ZipArchive, ZipArchiveMetadata.Empty);

            Assert.Equal(ZipArchiveProblems.None, result);
        }
    }
}
