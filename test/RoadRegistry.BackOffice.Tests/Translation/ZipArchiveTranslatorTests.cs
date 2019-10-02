namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using Xunit;

    public class ZipArchiveTranslatorTests
    {
        [Fact]
        public void EncodingCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ZipArchiveTranslator(null));
        }

        [Fact]
        public void IsZipArchiveTranslator()
        {
            var sut = new ZipArchiveTranslator(Encoding.UTF8);

            Assert.IsAssignableFrom<IZipArchiveTranslator>(sut);
        }

        [Fact]
        public void TranslateArchiveCanNotBeNull()
        {
            var sut = new ZipArchiveTranslator(Encoding.UTF8);

            Assert.Throws<ArgumentNullException>(() => sut.Translate(null));
        }

        [Theory]
        [MemberData(nameof(TranslateCases))]
        public void TranslateReturnsExpectedResult(ZipArchive archive, TranslatedChanges expected)
        {
            using (archive)
            {
                var sut = new ZipArchiveTranslator(Encoding.UTF8);

                var result = sut.Translate(archive);

                Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
            }
        }

        public static IEnumerable<object[]> TranslateCases
        {
            get
            {
                // Empty archive

                var stream1 = new MemoryStream();
                using (var archive1 = new ZipArchive(stream1, ZipArchiveMode.Create, true))
                {
                    archive1.CreateEntry(Guid.NewGuid().ToString("N"));
                }

                stream1.Position = 0;

                yield return new object[]
                {
                    new ZipArchive(stream1, ZipArchiveMode.Read, false),
                    TranslatedChanges.Empty
                };

                // Filled archive with files out of order

                var fixture = CreateFixture();

                var roadSegmentChangeDbaseRecord1 = fixture.Create<RoadSegmentChangeDbaseRecord>();
                roadSegmentChangeDbaseRecord1.RECORDTYPE.Value = RecordTypes.Added;
                var roadSegmentChangeDbaseRecord2 = fixture.Create<RoadSegmentChangeDbaseRecord>();
                roadSegmentChangeDbaseRecord2.RECORDTYPE.Value = RecordTypes.Added;
                var roadSegmentShapeChangeStream = new MemoryStream();
                var polyLineMShapeContent1 = fixture.Create<PolyLineMShapeContent>();
                var roadSegmentShapeChangeRecord1 =
                    polyLineMShapeContent1.RecordAs(RecordNumber.Initial);
                var polyLineMShapeContent2 = fixture.Create<PolyLineMShapeContent>();
                var roadSegmentShapeChangeRecord2 =
                    polyLineMShapeContent1.RecordAs(new RecordNumber(2));
                using (var writer = new ShapeBinaryWriter(
                    new ShapeFileHeader(
                        roadSegmentShapeChangeRecord1.Length.Plus(roadSegmentShapeChangeRecord2.Length)
                            .Plus(ShapeFileHeader.Length),
                        ShapeType.PolyLineM,
                        BoundingBox3D
                            .FromGeometry(polyLineMShapeContent1.Shape)
                            .ExpandWith(BoundingBox3D.FromGeometry(polyLineMShapeContent2.Shape))),
                    new BinaryWriter(
                        roadSegmentShapeChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(roadSegmentShapeChangeRecord1);
                    writer.Write(roadSegmentShapeChangeRecord2);
                }

                var roadSegmentDbaseChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(2),
                        RoadSegmentChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        roadSegmentDbaseChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(roadSegmentChangeDbaseRecord1);
                    writer.Write(roadSegmentChangeDbaseRecord2);
                }

                var europeanRoadChangeDbaseRecord = fixture.Create<EuropeanRoadChangeDbaseRecord>();
                europeanRoadChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                europeanRoadChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                var europeanRoadChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        EuropeanRoadChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        europeanRoadChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(europeanRoadChangeDbaseRecord);
                }

                var nationalRoadChangeDbaseRecord = fixture.Create<NationalRoadChangeDbaseRecord>();
                nationalRoadChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                nationalRoadChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                var nationalRoadChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        NationalRoadChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        nationalRoadChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(nationalRoadChangeDbaseRecord);
                }

                var numberedRoadChangeDbaseRecord = fixture.Create<NumberedRoadChangeDbaseRecord>();
                numberedRoadChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                numberedRoadChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                var numberedRoadChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        NumberedRoadChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        numberedRoadChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(numberedRoadChangeDbaseRecord);
                }

                var laneChangeDbaseRecord = fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
                laneChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                laneChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                laneChangeDbaseRecord.TOTPOSITIE.Value = laneChangeDbaseRecord.VANPOSITIE.Value + 1.0;
                var laneChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        RoadSegmentLaneChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        laneChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(laneChangeDbaseRecord);
                }

                var widthChangeDbaseRecord = fixture.Create<RoadSegmentWidthChangeDbaseRecord>();
                widthChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                widthChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                widthChangeDbaseRecord.TOTPOSITIE.Value = widthChangeDbaseRecord.VANPOSITIE.Value + 1.0;
                var widthChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        RoadSegmentWidthChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        widthChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(widthChangeDbaseRecord);
                }

                var surfaceChangeDbaseRecord = fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
                surfaceChangeDbaseRecord.WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                surfaceChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                surfaceChangeDbaseRecord.TOTPOSITIE.Value = surfaceChangeDbaseRecord.VANPOSITIE.Value + 1.0;
                var surfaceChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        RoadSegmentSurfaceChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        surfaceChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(surfaceChangeDbaseRecord);
                }

                var roadNodeChangeDbaseRecord1 = fixture.Create<RoadNodeChangeDbaseRecord>();
                roadNodeChangeDbaseRecord1.WEGKNOOPID.Value = roadSegmentChangeDbaseRecord1.B_WK_OIDN.Value;
                roadNodeChangeDbaseRecord1.RECORDTYPE.Value = RecordTypes.Added;
                var roadNodeChangeDbaseRecord2 = fixture.Create<RoadNodeChangeDbaseRecord>();
                roadNodeChangeDbaseRecord2.WEGKNOOPID.Value = roadSegmentChangeDbaseRecord1.E_WK_OIDN.Value;
                roadNodeChangeDbaseRecord2.RECORDTYPE.Value = RecordTypes.Added;
                var roadNodeChangeDbaseRecord3 = fixture.Create<RoadNodeChangeDbaseRecord>();
                roadNodeChangeDbaseRecord3.WEGKNOOPID.Value = roadSegmentChangeDbaseRecord2.B_WK_OIDN.Value;
                roadNodeChangeDbaseRecord3.RECORDTYPE.Value = RecordTypes.Added;
                var roadNodeChangeDbaseRecord4 = fixture.Create<RoadNodeChangeDbaseRecord>();
                roadNodeChangeDbaseRecord4.WEGKNOOPID.Value = roadSegmentChangeDbaseRecord2.E_WK_OIDN.Value;
                roadNodeChangeDbaseRecord4.RECORDTYPE.Value = RecordTypes.Added;

                var roadNodeShapeChangeStream = new MemoryStream();
                var pointShapeContent1 = fixture.Create<PointShapeContent>();
                var roadNodeShapeChangeRecord1 =
                    pointShapeContent1.RecordAs(RecordNumber.Initial);
                var pointShapeContent2 = fixture.Create<PointShapeContent>();
                var roadNodeShapeChangeRecord2 =
                    pointShapeContent2.RecordAs(new RecordNumber(2));
                var pointShapeContent3 = fixture.Create<PointShapeContent>();
                var roadNodeShapeChangeRecord3 =
                    pointShapeContent3.RecordAs(new RecordNumber(3));
                var pointShapeContent4 = fixture.Create<PointShapeContent>();
                var roadNodeShapeChangeRecord4 =
                    pointShapeContent4.RecordAs(new RecordNumber(4));
                using (var writer = new ShapeBinaryWriter(
                    new ShapeFileHeader(
                        roadNodeShapeChangeRecord1.Length
                            .Plus(roadNodeShapeChangeRecord2.Length)
                            .Plus(roadNodeShapeChangeRecord3.Length)
                            .Plus(roadNodeShapeChangeRecord4.Length)
                            .Plus(ShapeFileHeader.Length),
                        ShapeType.Point,
                        BoundingBox3D
                            .FromGeometry(pointShapeContent1.Shape)
                            .ExpandWith(BoundingBox3D.FromGeometry(pointShapeContent2.Shape))
                            .ExpandWith(BoundingBox3D.FromGeometry(pointShapeContent3.Shape))
                            .ExpandWith(BoundingBox3D.FromGeometry(pointShapeContent4.Shape))),
                    new BinaryWriter(
                        roadNodeShapeChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(roadNodeShapeChangeRecord1);
                    writer.Write(roadNodeShapeChangeRecord2);
                    writer.Write(roadNodeShapeChangeRecord3);
                    writer.Write(roadNodeShapeChangeRecord4);
                }

                var roadNodeDbaseChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(4),
                        RoadNodeChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        roadNodeDbaseChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(roadNodeChangeDbaseRecord1);
                    writer.Write(roadNodeChangeDbaseRecord2);
                    writer.Write(roadNodeChangeDbaseRecord3);
                    writer.Write(roadNodeChangeDbaseRecord4);
                }

                var gradeSeparatedJunctionChangeDbaseRecord = fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>();
                gradeSeparatedJunctionChangeDbaseRecord.BO_WS_OIDN.Value = roadSegmentChangeDbaseRecord1.WS_OIDN.Value;
                gradeSeparatedJunctionChangeDbaseRecord.ON_WS_OIDN.Value = roadSegmentChangeDbaseRecord2.WS_OIDN.Value;
                gradeSeparatedJunctionChangeDbaseRecord.RECORDTYPE.Value = RecordTypes.Added;
                var gradeSeparatedJunctionChangeStream = new MemoryStream();
                using (var writer = new DbaseBinaryWriter(
                    new DbaseFileHeader(
                        fixture.Create<DateTime>(),
                        DbaseCodePage.Western_European_ANSI,
                        new DbaseRecordCount(1),
                        GradeSeparatedJunctionChangeDbaseRecord.Schema),
                    new BinaryWriter(
                        gradeSeparatedJunctionChangeStream,
                        Encoding.UTF8,
                        true)))
                {
                    writer.Write(gradeSeparatedJunctionChangeDbaseRecord);
                }

                var random = new Random(fixture.Create<int>());
                var writeOrder =
                    new[] {
                        "WEGSEGMENT_ALL.SHP",
                        "WEGKNOOP_ALL.SHP",
                        "ATTEUROPWEG_ALL.DBF",
                        "ATTGENUMWEG_ALL.DBF",
                        "ATTNATIONWEG_ALL.DBF",
                        "ATTRIJSTROKEN_ALL.DBF",
                        "ATTWEGBREEDTE_ALL.DBF",
                        "ATTWEGVERHARDING_ALL.DBF",
                        "RLTOGKRUISING_ALL.DBF",
                        "WEGSEGMENT_ALL.DBF",
                        "WEGKNOOP_ALL.DBF"
                    }
                    .OrderBy(_ => random.Next()) // sort randomly
                    .ToArray();

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

                var archiveStream = new MemoryStream();
                using (var createArchive =
                    new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    foreach (var file in writeOrder)
                    {
                        switch (file)
                        {
                            case "WEGSEGMENT_ALL.SHP":
                                using (var entryStream =
                                    createArchive.CreateEntry("WEGSEGMENT_ALL.SHP").Open())
                                {
                                    roadSegmentShapeChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "WEGSEGMENT_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("WEGSEGMENT_ALL.DBF").Open())
                                {
                                    roadSegmentDbaseChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "WEGKNOOP_ALL.SHP":
                                using (var entryStream =
                                    createArchive.CreateEntry("WEGKNOOP_ALL.SHP").Open())
                                {
                                    roadNodeShapeChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "WEGKNOOP_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("WEGKNOOP_ALL.DBF").Open())
                                {
                                    roadNodeDbaseChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "ATTEUROPWEG_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("ATTEUROPWEG_ALL.DBF").Open())
                                {
                                    europeanRoadChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "ATTGENUMWEG_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("ATTGENUMWEG_ALL.DBF").Open())
                                {
                                    numberedRoadChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "ATTNATIONWEG_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("ATTNATIONWEG_ALL.DBF").Open())
                                {
                                    nationalRoadChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "ATTRIJSTROKEN_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("ATTRIJSTROKEN_ALL.DBF").Open())
                                {
                                    laneChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "ATTWEGBREEDTE_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("ATTWEGBREEDTE_ALL.DBF").Open())
                                {
                                    widthChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "ATTWEGVERHARDING_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("ATTWEGVERHARDING_ALL.DBF").Open())
                                {
                                    surfaceChangeStream.CopyTo(entryStream);
                                }

                                break;
                            case "RLTOGKRUISING_ALL.DBF":
                                using (var entryStream =
                                    createArchive.CreateEntry("RLTOGKRUISING_ALL.DBF").Open())
                                {
                                    gradeSeparatedJunctionChangeStream.CopyTo(entryStream);
                                }

                                break;
                        }
                    }
                }

                archiveStream.Position = 0;

                yield return new object[]
                {
                    new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8),
                    TranslatedChanges.Empty
                        .Append(
                            new AddRoadNode(
                                new RecordNumber(1),
                                new RoadNodeId(roadNodeChangeDbaseRecord1.WEGKNOOPID.Value.GetValueOrDefault()),
                                RoadNodeType.ByIdentifier[roadNodeChangeDbaseRecord1.TYPE.Value.GetValueOrDefault()]
                            ).WithGeometry(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPoint(((PointShapeContent)roadNodeShapeChangeRecord1.Content).Shape))
                        )
                        .Append(
                            new AddRoadNode(
                                new RecordNumber(2),
                                new RoadNodeId(roadNodeChangeDbaseRecord2.WEGKNOOPID.Value.GetValueOrDefault()),
                                RoadNodeType.ByIdentifier[roadNodeChangeDbaseRecord2.TYPE.Value.GetValueOrDefault()]
                            ).WithGeometry(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPoint(((PointShapeContent)roadNodeShapeChangeRecord2.Content).Shape))
                        )
                        .Append(
                            new AddRoadNode(
                                new RecordNumber(3),
                                new RoadNodeId(roadNodeChangeDbaseRecord3.WEGKNOOPID.Value.GetValueOrDefault()),
                                RoadNodeType.ByIdentifier[roadNodeChangeDbaseRecord3.TYPE.Value.GetValueOrDefault()]
                            ).WithGeometry(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPoint(((PointShapeContent)roadNodeShapeChangeRecord3.Content).Shape))
                        )
                        .Append(
                            new AddRoadNode(
                                new RecordNumber(4),
                                new RoadNodeId(roadNodeChangeDbaseRecord4.WEGKNOOPID.Value.GetValueOrDefault()),
                                RoadNodeType.ByIdentifier[roadNodeChangeDbaseRecord4.TYPE.Value.GetValueOrDefault()]
                            ).WithGeometry(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPoint(((PointShapeContent)roadNodeShapeChangeRecord4.Content).Shape))
                        )
                        .Append(
                            new AddRoadSegment(
                                new RecordNumber(2),
                                new RoadSegmentId(roadSegmentChangeDbaseRecord2.WS_OIDN.Value.GetValueOrDefault()),
                                new RoadNodeId(roadSegmentChangeDbaseRecord2.B_WK_OIDN.Value.GetValueOrDefault()),
                                new RoadNodeId(roadSegmentChangeDbaseRecord2.E_WK_OIDN.Value.GetValueOrDefault()),
                                new MaintenanceAuthorityId(roadSegmentChangeDbaseRecord2.BEHEERDER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentChangeDbaseRecord2.METHODE.Value.GetValueOrDefault()],
                                RoadSegmentMorphology.ByIdentifier[roadSegmentChangeDbaseRecord2.MORFOLOGIE.Value.GetValueOrDefault()],
                                RoadSegmentStatus.ByIdentifier[roadSegmentChangeDbaseRecord2.STATUS.Value.GetValueOrDefault()],
                                RoadSegmentCategory.ByIdentifier[roadSegmentChangeDbaseRecord2.WEGCAT.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[roadSegmentChangeDbaseRecord2.TGBEP.Value.GetValueOrDefault()],
                                roadSegmentChangeDbaseRecord2.LSTRNMID.Value.HasValue ? new CrabStreetnameId(roadSegmentChangeDbaseRecord2.LSTRNMID.Value.GetValueOrDefault()) : default,
                                roadSegmentChangeDbaseRecord2.RSTRNMID.Value.HasValue ? new CrabStreetnameId(roadSegmentChangeDbaseRecord2.RSTRNMID.Value.GetValueOrDefault()) : default
                            ).WithGeometry(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryMultiLineString(((PolyLineMShapeContent)roadSegmentShapeChangeRecord2.Content).Shape))
                        )
                        .Append(
                            new AddRoadSegmentToEuropeanRoad
                            (
                                new AttributeId(europeanRoadChangeDbaseRecord.EU_OIDN.Value.GetValueOrDefault()),
                                new RoadSegmentId(europeanRoadChangeDbaseRecord.WS_OIDN.Value.GetValueOrDefault()),
                                EuropeanRoadNumber.Parse(europeanRoadChangeDbaseRecord.EUNUMMER.Value)
                            )
                        )
                        .Append(
                            new AddRoadSegmentToNationalRoad
                            (
                                new AttributeId(nationalRoadChangeDbaseRecord.NW_OIDN.Value.GetValueOrDefault()),
                                new RoadSegmentId(nationalRoadChangeDbaseRecord.WS_OIDN.Value.GetValueOrDefault()),
                                NationalRoadNumber.Parse(nationalRoadChangeDbaseRecord.IDENT2.Value)
                            )
                        )
                        .Append(
                            new AddRoadSegmentToNumberedRoad
                            (
                                new AttributeId(numberedRoadChangeDbaseRecord.GW_OIDN.Value.GetValueOrDefault()),
                                new RoadSegmentId(numberedRoadChangeDbaseRecord.WS_OIDN.Value.GetValueOrDefault()),
                                NumberedRoadNumber.Parse(numberedRoadChangeDbaseRecord.IDENT8.Value),
                                RoadSegmentNumberedRoadDirection.ByIdentifier[numberedRoadChangeDbaseRecord.RICHTING.Value.GetValueOrDefault()],
                                new RoadSegmentNumberedRoadOrdinal(numberedRoadChangeDbaseRecord.VOLGNUMMER.Value.GetValueOrDefault())
                            )
                        )
                        .Append(
                            new AddRoadSegment(
                                    new RecordNumber(1),
                                    new RoadSegmentId(roadSegmentChangeDbaseRecord1.WS_OIDN.Value.GetValueOrDefault()),
                                    new RoadNodeId(roadSegmentChangeDbaseRecord1.B_WK_OIDN.Value.GetValueOrDefault()),
                                    new RoadNodeId(roadSegmentChangeDbaseRecord1.E_WK_OIDN.Value.GetValueOrDefault()),
                                    new MaintenanceAuthorityId(roadSegmentChangeDbaseRecord1.BEHEERDER.Value),
                                    RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentChangeDbaseRecord1.METHODE.Value.GetValueOrDefault()],
                                    RoadSegmentMorphology.ByIdentifier[roadSegmentChangeDbaseRecord1.MORFOLOGIE.Value.GetValueOrDefault()],
                                    RoadSegmentStatus.ByIdentifier[roadSegmentChangeDbaseRecord1.STATUS.Value.GetValueOrDefault()],
                                    RoadSegmentCategory.ByIdentifier[roadSegmentChangeDbaseRecord1.WEGCAT.Value],
                                    RoadSegmentAccessRestriction.ByIdentifier[roadSegmentChangeDbaseRecord1.TGBEP.Value.GetValueOrDefault()],
                                    roadSegmentChangeDbaseRecord1.LSTRNMID.Value.HasValue ? new CrabStreetnameId(roadSegmentChangeDbaseRecord1.LSTRNMID.Value.GetValueOrDefault()) : default,
                                    roadSegmentChangeDbaseRecord1.RSTRNMID.Value.HasValue ? new CrabStreetnameId(roadSegmentChangeDbaseRecord1.RSTRNMID.Value.GetValueOrDefault()) : default
                                )
                                .WithGeometry(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryMultiLineString(((PolyLineMShapeContent)roadSegmentShapeChangeRecord1.Content).Shape))
                                .WithLane(
                                    new RoadSegmentLaneAttribute(
                                        new AttributeId(laneChangeDbaseRecord.RS_OIDN.Value.GetValueOrDefault()),
                                        new RoadSegmentLaneCount(laneChangeDbaseRecord.AANTAL.Value.GetValueOrDefault()),
                                        RoadSegmentLaneDirection.ByIdentifier[laneChangeDbaseRecord.RICHTING.Value.GetValueOrDefault()],
                                        new RoadSegmentPosition(Convert.ToDecimal(laneChangeDbaseRecord.VANPOSITIE.Value.GetValueOrDefault())),
                                        new RoadSegmentPosition(Convert.ToDecimal(laneChangeDbaseRecord.TOTPOSITIE.Value.GetValueOrDefault()))
                                    )
                                )
                                .WithWidth(
                                    new RoadSegmentWidthAttribute(
                                        new AttributeId(widthChangeDbaseRecord.WB_OIDN.Value.GetValueOrDefault()),
                                        new RoadSegmentWidth(widthChangeDbaseRecord.BREEDTE.Value.GetValueOrDefault()),
                                        new RoadSegmentPosition(Convert.ToDecimal(widthChangeDbaseRecord.VANPOSITIE.Value.GetValueOrDefault())),
                                        new RoadSegmentPosition(Convert.ToDecimal(widthChangeDbaseRecord.TOTPOSITIE.Value.GetValueOrDefault()))
                                    )
                                )
                                .WithSurface(
                                    new RoadSegmentSurfaceAttribute(
                                        new AttributeId(surfaceChangeDbaseRecord.WV_OIDN.Value.GetValueOrDefault()),
                                        RoadSegmentSurfaceType.ByIdentifier[surfaceChangeDbaseRecord.TYPE.Value.GetValueOrDefault()],
                                        new RoadSegmentPosition(Convert.ToDecimal(surfaceChangeDbaseRecord.VANPOSITIE.Value.GetValueOrDefault())),
                                        new RoadSegmentPosition(Convert.ToDecimal(surfaceChangeDbaseRecord.TOTPOSITIE.Value.GetValueOrDefault()))
                                    )
                                )
                        )
                        .Append(
                            new AddGradeSeparatedJunction
                            (
                                new GradeSeparatedJunctionId(gradeSeparatedJunctionChangeDbaseRecord.OK_OIDN.Value.GetValueOrDefault()),
                                GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionChangeDbaseRecord.TYPE.Value.GetValueOrDefault()],
                                new RoadSegmentId(gradeSeparatedJunctionChangeDbaseRecord.BO_WS_OIDN.Value.GetValueOrDefault()),
                                new RoadSegmentId(gradeSeparatedJunctionChangeDbaseRecord.ON_WS_OIDN.Value.GetValueOrDefault())
                            )
                        )
                };
            }
        }

        private static Fixture CreateFixture()
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

            fixture.Customize<NetTopologySuite.Geometries.Point>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.Point(
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
                        Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(fixture.Create<NetTopologySuite.Geometries.Point>())))
                    .OmitAutoProperties()
            );

            fixture.Customize<NetTopologySuite.Geometries.LineString>(customization =>
                customization.FromFactory(generator =>
                    new LineString(
                        new CoordinateArraySequence(
                            new[]
                            {
                                new Coordinate(0.0, 0.0),
                                new Coordinate(1.0, 1.0),
                            }),
                        Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryConfiguration.GeometryFactory
                    )
                ).OmitAutoProperties()
            );

            fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(new[] {fixture.Create<LineString>()})
                ).OmitAutoProperties()
            );
            fixture.Customize<PolyLineMShapeContent>(customization =>
                customization
                    .FromFactory(random => new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(fixture.Create<MultiLineString>())))
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
            return fixture;
        }
    }
}
