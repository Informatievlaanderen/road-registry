namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using GeoAPI.Geometries;
    using Model;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class ZipArchiveValidatorTests
    {
        [Fact]
        public void EncodingCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ZipArchiveValidator(null));
        }

        [Theory]
        [MemberData(nameof(MissingRequiredFileCases))]
        public void ValidateReturnsExpectedResultWhenRequiredFileMissing(ZipArchive archive, ZipArchiveErrors expected)
        {
            using (archive)
            {
                var sut = new ZipArchiveValidator(Encoding.UTF8);

                var result = sut.Validate(archive);

                Assert.Equal(expected, result);
            }
        }

        public static IEnumerable<object[]> MissingRequiredFileCases
        {
            get
            {
                var fixture = new Fixture();

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
                fixture.CustomizeMaintenanceAuthorityId();
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

                fixture.Customize<EuropeanRoadChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new EuropeanRoadChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            EU_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                            WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            EUNUMMER = {Value = fixture.Create<EuropeanRoadNumber>().ToString()}
                        })
                        .OmitAutoProperties());

                fixture.Customize<GradeSeparatedJunctionChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new GradeSeparatedJunctionChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            OK_OIDN = {Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue))},
                            TYPE =
                                {Value = (short) fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier},
                            BO_WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            ON_WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                        })
                        .OmitAutoProperties());

                fixture.Customize<NationalRoadChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new NationalRoadChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            NW_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                            WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            IDENT2 = {Value = fixture.Create<NationalRoadNumber>().ToString()}
                        })
                        .OmitAutoProperties());

                fixture.Customize<NumberedRoadChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new NumberedRoadChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            GW_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                            WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            IDENT8 = {Value = fixture.Create<NumberedRoadNumber>().ToString()},
                            RICHTING =
                            {
                                Value = (short) fixture.Create<RoadSegmentNumberedRoadDirection>().Translation
                                    .Identifier
                            },
                            VOLGNUMMER = {Value = fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32()}
                        })
                        .OmitAutoProperties());

                fixture.Customize<RoadNodeChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new RoadNodeChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            WEGKNOOPID = {Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                            TYPE = {Value = (short) fixture.Create<RoadNodeType>().Translation.Identifier}
                        })
                        .OmitAutoProperties());

                fixture.Customize<PointM>(customization =>
                    customization.FromFactory(generator =>
                        new PointM(
                            fixture.Create<double>(),
                            fixture.Create<double>(),
                            fixture.Create<double>(),
                            fixture.Create<double>()
                        )
                    ).OmitAutoProperties()
                );

                fixture.Customize<RecordNumber>(customizer =>
                    customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));

                fixture.Customize<PointShapeContent>(customization =>
                    customization
                        .FromFactory(random => new PointShapeContent(fixture.Create<PointM>()))
                        .OmitAutoProperties()
                );

                fixture.Customize<ILineString>(customization =>
                    customization.FromFactory(generator =>
                        new LineString(
                            new PointSequence(
                                new[]
                                {
                                    new PointM(0.0, 0.0),
                                    new PointM(1.0, 1.0)
                                }),
                            GeometryConfiguration.GeometryFactory
                        )
                    ).OmitAutoProperties()
                );

                fixture.Customize<MultiLineString>(customization =>
                    customization.FromFactory(generator =>
                        new MultiLineString(new[] {fixture.Create<ILineString>()})
                    ).OmitAutoProperties()
                );
                fixture.Customize<PolyLineMShapeContent>(customization =>
                    customization
                        .FromFactory(random => new PolyLineMShapeContent(fixture.Create<MultiLineString>()))
                        .OmitAutoProperties()
                );

                fixture.Customize<RoadSegmentChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new RoadSegmentChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            WS_OIDN = {Value = new RoadSegmentId(random.Next(1, int.MaxValue))},
                            METHODE =
                            {
                                Value = (short) fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier
                            },
                            BEHEERDER = {Value = fixture.Create<MaintenanceAuthorityId>()},
                            MORFOLOGIE =
                                {Value = (short) fixture.Create<RoadSegmentMorphology>().Translation.Identifier},
                            STATUS = {Value = fixture.Create<RoadSegmentStatus>().Translation.Identifier},
                            WEGCAT = {Value = fixture.Create<RoadSegmentCategory>().Translation.Identifier},
                            B_WK_OIDN = {Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                            E_WK_OIDN = {Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                            LSTRNMID = {Value = new CrabStreetnameId(random.Next(1, int.MaxValue))},
                            RSTRNMID = {Value = new CrabStreetnameId(random.Next(1, int.MaxValue))},
                            TGBEP =
                            {
                                Value = (short) fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier
                            },
                            EVENTIDN = {Value = new RoadSegmentId(random.Next(1, int.MaxValue))}
                        })
                        .OmitAutoProperties());

                fixture.Customize<RoadSegmentLaneChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new RoadSegmentLaneChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            RS_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                            WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            VANPOSITIE = {Value = fixture.Create<RoadSegmentPosition>().ToDouble()},
                            TOTPOSITIE = {Value = fixture.Create<RoadSegmentPosition>().ToDouble()},
                            AANTAL = {Value = (short) fixture.Create<RoadSegmentLaneCount>().ToInt32()},
                            RICHTING =
                            {
                                Value = (short) fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier
                            }
                        })
                        .OmitAutoProperties());

                fixture.Customize<RoadSegmentSurfaceChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new RoadSegmentSurfaceChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            WV_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                            WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            VANPOSITIE = {Value = fixture.Create<RoadSegmentPosition>().ToDouble()},
                            TOTPOSITIE = {Value = fixture.Create<RoadSegmentPosition>().ToDouble()},
                            TYPE = {Value = (short) fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier}
                        })
                        .OmitAutoProperties());
                fixture.Customize<RoadSegmentWidthChangeDbaseRecord>(
                    composer => composer
                        .FromFactory(random => new RoadSegmentWidthChangeDbaseRecord
                        {
                            RECORDTYPE = {Value = (short) random.Next(1, 5)},
                            TRANSACTID = {Value = (short) random.Next(1, 9999)},
                            WB_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                            WS_OIDN = {Value = fixture.Create<RoadSegmentId>().ToInt32()},
                            VANPOSITIE = {Value = fixture.Create<RoadSegmentPosition>().ToDouble()},
                            TOTPOSITIE = {Value = fixture.Create<RoadSegmentPosition>().ToDouble()},
                            BREEDTE = {Value = (short) fixture.Create<RoadSegmentWidth>().ToInt32()}
                        })
                        .OmitAutoProperties());

                var roadSegmentShapeChangeStream = new MemoryStream();
                var polyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
                var roadSegmentShapeChangeRecord =
                    polyLineMShapeContent.RecordAs(fixture.Create<RecordNumber>());
                new ShapeBinaryWriter(
                        new ShapeFileHeader(
                            roadSegmentShapeChangeRecord.Length.Plus(ShapeFileHeader.Length),
                            ShapeType.PolyLineM,
                            BoundingBox3D.FromGeometry(polyLineMShapeContent.Shape)),
                        new BinaryWriter(
                            roadSegmentShapeChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(roadSegmentShapeChangeRecord);
                var roadSegmentDbaseChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            RoadSegmentChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            roadSegmentDbaseChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<RoadSegmentChangeDbaseRecord>());
                var europeanRoadChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            EuropeanRoadChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            europeanRoadChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<EuropeanRoadChangeDbaseRecord>());
                var nationalRoadChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            NationalRoadChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            nationalRoadChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<NationalRoadChangeDbaseRecord>());
                var numberedRoadChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            NumberedRoadChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            numberedRoadChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<NumberedRoadChangeDbaseRecord>());
                var laneChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            RoadSegmentLaneChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            laneChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<RoadSegmentLaneChangeDbaseRecord>());
                var widthChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            RoadSegmentWidthChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            widthChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<RoadSegmentWidthChangeDbaseRecord>());
                var surfaceChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            RoadSegmentSurfaceChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            surfaceChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>());
                var roadNodeShapeChangeStream = new MemoryStream();
                var pointShapeContent = fixture.Create<PointShapeContent>();
                var roadNodeShapeChangeRecord =
                    pointShapeContent.RecordAs(fixture.Create<RecordNumber>());
                new ShapeBinaryWriter(
                        new ShapeFileHeader(
                            roadNodeShapeChangeRecord.Length.Plus(ShapeFileHeader.Length),
                            ShapeType.Point,
                            BoundingBox3D.FromGeometry(pointShapeContent.Shape)),
                        new BinaryWriter(
                            roadNodeShapeChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(roadNodeShapeChangeRecord);
                var roadNodeDbaseChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            RoadNodeChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            roadNodeDbaseChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<RoadNodeChangeDbaseRecord>());
                var gradeSeparatedJunctionChangeStream = new MemoryStream();
                new DbaseBinaryWriter(
                        new DbaseFileHeader(
                            fixture.Create<DateTime>(),
                            DbaseCodePage.Western_European_ANSI,
                            new DbaseRecordCount(1),
                            GradeSeparatedJunctionChangeDbaseRecord.Schema),
                        new BinaryWriter(
                            gradeSeparatedJunctionChangeStream,
                            Encoding.UTF8,
                            true))
                    .Write(fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>());

                var requiredFiles = new[]
                {
                    "WEGSEGMENT_ALL.SHP",
                    "WEGSEGMENT_ALL.DBF",
                    "WEGKNOOP_ALL.SHP",
                    "WEGKNOOP_ALL.DBF",
                    "ATTEUROPWEG_ALL.DBF",
                    "ATTGENUMWEG_ALL.DBF",
                    "ATTNATIONWEG_ALL.DBF",
                    "ATTRIJSTROKEN_ALL.DBF",
                    "ATTWEGBREEDTE_ALL.DBF",
                    "ATTWEGVERHARDING_ALL.DBF",
                    "RLTOGKRUISING_ALL.DBF"
                };

                for (var index = 0; index < requiredFiles.Length; index++)
                {
                    roadSegmentShapeChangeStream.Position = 0;
                    roadSegmentDbaseChangeStream.Position = 0;
                    europeanRoadChangeStream.Position = 0;
                    nationalRoadChangeStream.Position = 0;
                    numberedRoadChangeStream.Position = 0;
                    laneChangeStream.Position = 0;
                    widthChangeStream.Position = 0;
                    surfaceChangeStream.Position = 0;
                    roadNodeShapeChangeStream.Position = 0;
                    roadNodeDbaseChangeStream.Position = 0;
                    gradeSeparatedJunctionChangeStream.Position = 0;

                    var errors = ZipArchiveErrors.None;
                    var archiveStream = new MemoryStream();
                    using (var createArchive =
                        new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        foreach (var requiredFile in requiredFiles)
                        {
                            switch (requiredFile)
                            {
                                case "WEGSEGMENT_ALL.SHP":
                                    if (requiredFiles[index] == "WEGSEGMENT_ALL.SHP")
                                    {
                                        errors = errors.RequiredFileMissing("WEGSEGMENT_ALL.SHP");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("WEGSEGMENT_ALL.SHP").Open())
                                        {
                                            roadSegmentShapeChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "WEGSEGMENT_ALL.DBF":
                                    if (requiredFiles[index] == "WEGSEGMENT_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("WEGSEGMENT_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("WEGSEGMENT_ALL.DBF").Open())
                                        {
                                            roadSegmentDbaseChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "WEGKNOOP_ALL.SHP":
                                    if (requiredFiles[index] == "WEGKNOOP_ALL.SHP")
                                    {
                                        errors = errors.RequiredFileMissing("WEGKNOOP_ALL.SHP");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("WEGKNOOP_ALL.SHP").Open())
                                        {
                                            roadNodeShapeChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "WEGKNOOP_ALL.DBF":
                                    if (requiredFiles[index] == "WEGKNOOP_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("WEGKNOOP_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("WEGKNOOP_ALL.DBF").Open())
                                        {
                                            roadNodeDbaseChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "ATTEUROPWEG_ALL.DBF":
                                    if (requiredFiles[index] == "ATTEUROPWEG_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("ATTEUROPWEG_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("ATTEUROPWEG_ALL.DBF").Open())
                                        {
                                            europeanRoadChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "ATTGENUMWEG_ALL.DBF":
                                    if (requiredFiles[index] == "ATTGENUMWEG_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("ATTGENUMWEG_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("ATTGENUMWEG_ALL.DBF").Open())
                                        {
                                            numberedRoadChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "ATTNATIONWEG_ALL.DBF":
                                    if (requiredFiles[index] == "ATTNATIONWEG_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("ATTNATIONWEG_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("ATTNATIONWEG_ALL.DBF").Open())
                                        {
                                            nationalRoadChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "ATTRIJSTROKEN_ALL.DBF":
                                    if (requiredFiles[index] == "ATTRIJSTROKEN_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("ATTRIJSTROKEN_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("ATTRIJSTROKEN_ALL.DBF").Open())
                                        {
                                            laneChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "ATTWEGBREEDTE_ALL.DBF":
                                    if (requiredFiles[index] == "ATTWEGBREEDTE_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("ATTWEGBREEDTE_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("ATTWEGBREEDTE_ALL.DBF").Open())
                                        {
                                            widthChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "ATTWEGVERHARDING_ALL.DBF":
                                    if (requiredFiles[index] == "ATTWEGVERHARDING_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("ATTWEGVERHARDING_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("ATTWEGVERHARDING_ALL.DBF").Open())
                                        {
                                            surfaceChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                                case "RLTOGKRUISING_ALL.DBF":
                                    if (requiredFiles[index] == "RLTOGKRUISING_ALL.DBF")
                                    {
                                        errors = errors.RequiredFileMissing("RLTOGKRUISING_ALL.DBF");
                                    }
                                    else
                                    {
                                        using (var entryStream =
                                            createArchive.CreateEntry("RLTOGKRUISING_ALL.DBF").Open())
                                        {
                                            gradeSeparatedJunctionChangeStream.CopyTo(entryStream);
                                        }
                                    }

                                    break;
                            }
                        }
                    }

                    archiveStream.Position = 0;

                    yield return new object[]
                    {
                        new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8),
                        errors
                    };
                }
            }
        }
    }
}
