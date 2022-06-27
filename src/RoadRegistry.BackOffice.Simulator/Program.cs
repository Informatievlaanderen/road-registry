namespace RoadRegistry.BackOffice.Simulator
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Uploads;
    using Uploads.Schema;
    using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

    public class Program
    {
        protected Program()
        { }
        
        public static void Main()
        {
            Directory.CreateDirectory("output");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var node1Record = new RoadNodeChangeDbaseRecord { TYPE = { Value = (short)RoadNodeType.EndNode.Translation.Identifier },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier },
                WEGKNOOPID = { Value = 1 }
            };
            var node1Shape = new PointShapeContent(new Point(0.0, 0.0));

            var node2Record = new RoadNodeChangeDbaseRecord { TYPE = { Value = (short)RoadNodeType.EndNode.Translation.Identifier },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier },
                WEGKNOOPID = { Value = 2 }
            };
            var node2Shape = new PointShapeContent(new Point(0.0, 1.0));

            var segment1Record = new RoadSegmentChangeDbaseRecord { WS_OIDN = { Value = 1 },
                B_WK_OIDN = { Value = 1 },
                E_WK_OIDN = { Value = 2 },
                TGBEP = { Value = (short) RoadSegmentAccessRestriction.PublicRoad.Translation.Identifier },
                STATUS = { Value = RoadSegmentStatus.InUse.Translation.Identifier },
                CATEGORIE = { Value = RoadSegmentCategory.SecondaryRoad.Translation.Identifier },
                METHODE = { Value = (short)RoadSegmentGeometryDrawMethod.Measured.Translation.Identifier },
                MORFOLOGIE = { Value = (short)RoadSegmentMorphology.Motorway.Translation.Identifier },
                LSTRNMID = { Value = 123 },
                RSTRNMID = { Value = 456 },
                BEHEERDER = { Value = "-8" },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };
            var segmentShape = new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(new NetTopologySuite.Geometries.MultiLineString(new[]
            {
                new NetTopologySuite.Geometries.LineString(new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(new[]
                {
                    new NetTopologySuite.Geometries.CoordinateM(0.0, 0.0, 0.0),
                    new NetTopologySuite.Geometries.CoordinateM(0.0, 1.0, 1.0)
                }), GeometryConfiguration.GeometryFactory)
            })));

            var laneRecord = new RoadSegmentLaneChangeDbaseRecord { RS_OIDN = { Value = 1 },
                WS_OIDN = { Value = 1 },
                RICHTING = { Value = (short)RoadSegmentLaneDirection.Independent.Translation.Identifier },
                AANTAL = { Value = 2 },
                VANPOSITIE = { Value = 0.0 },
                TOTPOSITIE = { Value = 1.0 },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var surfaceRecord = new RoadSegmentSurfaceChangeDbaseRecord { WV_OIDN = { Value = 1 },
                WS_OIDN = { Value = 1 },
                TYPE = { Value = (short) RoadSegmentSurfaceType.SolidSurface.Translation.Identifier },
                VANPOSITIE = { Value = 0.0 },
                TOTPOSITIE = { Value = 1.0 },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var widthRecord = new RoadSegmentWidthChangeDbaseRecord { WB_OIDN = { Value = 1 },
                WS_OIDN = { Value = 1 },
                BREEDTE = { Value = 1 },
                VANPOSITIE = { Value = 0.0 },
                TOTPOSITIE = { Value = 1.0 },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var nodeDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(2),
                new RoadNodeChangeDbaseSchema());
            var nodeShapeHeader = new ShapeFileHeader(
                ShapeFileHeader.Length.Plus(node1Shape.ContentLength).Plus(node2Shape.ContentLength),
                ShapeType.Point,
                BoundingBox3D
                    .FromGeometry(node1Shape.Shape)
                    .ExpandWith(
                        BoundingBox3D.FromGeometry(node2Shape.Shape)
                    )
                );

            // node

            using (var nodeDbaseFile = new BinaryWriter(File.OpenWrite("output/WEGKNOOP_ALL.dbf")))
            using (var nodeDbaseWriter = new DbaseBinaryWriter(nodeDbaseHeader, nodeDbaseFile))
            {
                nodeDbaseWriter.Write(node1Record);
                nodeDbaseWriter.Write(node2Record);
            }
            using (var nodeShapeIndexFile = new BinaryWriter(File.OpenWrite("output/WEGKNOOP_ALL.shx")))
            using (var nodeShapeFile = new BinaryWriter(File.OpenWrite("output/WEGKNOOP_ALL.shp")))
            using (var nodeShapeWriter = new ShapeBinaryWriter(nodeShapeHeader, nodeShapeFile))
            using (var nodeShapeIndexWriter =
                new ShapeIndexBinaryWriter(nodeShapeHeader.ForIndex(new ShapeRecordCount(2)), nodeShapeIndexFile))
            {
                var node1ShapeRecord = node1Shape.RecordAs(RecordNumber.Initial);
                nodeShapeWriter.Write(node1ShapeRecord);
                var node2ShapeRecord = node2Shape.RecordAs(RecordNumber.Initial.Next());
                nodeShapeWriter.Write(node2ShapeRecord);

                var offset = ShapeIndexRecord.InitialOffset;
                var node1ShapeIndexRecord = node1ShapeRecord.IndexAt(offset);
                var node2ShapeIndexRecord = node2ShapeRecord.IndexAt(offset.Plus(node1ShapeRecord.Length));
                nodeShapeIndexWriter.Write(node1ShapeIndexRecord);
                nodeShapeIndexWriter.Write(node2ShapeIndexRecord);
            }

            // segment

            var segmentDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new RoadSegmentChangeDbaseSchema());
            var segmentShapeHeader = new ShapeFileHeader(
                ShapeFileHeader.Length.Plus(segmentShape.ContentLength),
                ShapeType.PolyLineM,
                BoundingBox3D
                    .FromGeometry(segmentShape.Shape)
            );

            using (var segmentDbaseFile = new BinaryWriter(File.OpenWrite("output/WEGSEGMENT_ALL.dbf")))
            using (var segmentDbaseWriter = new DbaseBinaryWriter(segmentDbaseHeader, segmentDbaseFile))
            {
                segmentDbaseWriter.Write(segment1Record);
            }
            using (var segmentShapeIndexFile = new BinaryWriter(File.OpenWrite("output/WEGSEGMENT_ALL.shx")))
            using (var segmentShapeFile = new BinaryWriter(File.OpenWrite("output/WEGSEGMENT_ALL.shp")))
            using (var segmentShapeWriter = new ShapeBinaryWriter(segmentShapeHeader, segmentShapeFile))
            using (var segmentShapeIndexWriter =
                new ShapeIndexBinaryWriter(segmentShapeHeader.ForIndex(new ShapeRecordCount(2)), segmentShapeIndexFile))
            {
                var segment1ShapeRecord = segmentShape.RecordAs(RecordNumber.Initial);
                segmentShapeWriter.Write(segment1ShapeRecord);

                var offset = ShapeIndexRecord.InitialOffset;
                var segment1ShapeIndexRecord = segment1ShapeRecord.IndexAt(offset);
                segmentShapeIndexWriter.Write(segment1ShapeIndexRecord);
            }

            // lane
            var laneDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new RoadSegmentLaneChangeDbaseSchema());

            using (var laneDbaseFile = new BinaryWriter(File.OpenWrite("output/ATTRIJSTROKEN_ALL.DBF")))
            using (var laneDbaseWriter = new DbaseBinaryWriter(laneDbaseHeader, laneDbaseFile))
            {
                laneDbaseWriter.Write(laneRecord);
            }

            // width
            var widthDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new RoadSegmentWidthChangeDbaseSchema());

            using (var widthDbaseFile = new BinaryWriter(File.OpenWrite("output/ATTWEGBREEDTE_ALL.DBF")))
            using (var widthDbaseWriter = new DbaseBinaryWriter(widthDbaseHeader, widthDbaseFile))
            {
                widthDbaseWriter.Write(widthRecord);
            }

            // surface
            var surfaceDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new RoadSegmentSurfaceChangeDbaseSchema());

            using (var surfaceDbaseFile = new BinaryWriter(File.OpenWrite("output/ATTWEGVERHARDING_ALL.DBF")))
            using (var surfaceDbaseWriter = new DbaseBinaryWriter(surfaceDbaseHeader, surfaceDbaseFile))
            {
                surfaceDbaseWriter.Write(surfaceRecord);
            }

            // european road
            var europeanRoadRecord = new EuropeanRoadChangeDbaseRecord { EU_OIDN = { Value = 1 },
                WS_OIDN = { Value = 1 },
                EUNUMMER = { Value = EuropeanRoadNumber.E40.ToString() },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var europeanDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new EuropeanRoadChangeDbaseSchema());

            using (var europeanDbaseFile = new BinaryWriter(File.OpenWrite("output/ATTEUROPWEG_ALL.DBF")))
            using (var europeanDbaseWriter = new DbaseBinaryWriter(europeanDbaseHeader, europeanDbaseFile))
            {
                europeanDbaseWriter.Write(europeanRoadRecord);
            }

            // national road
            var nationalRoadRecord = new NationalRoadChangeDbaseRecord { NW_OIDN = { Value = 1 },
                WS_OIDN = { Value = 1 },
                IDENT2 = { Value = NationalRoadNumber.Parse("R0").ToString() },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var nationalDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new NationalRoadChangeDbaseSchema());

            using (var nationalDbaseFile = new BinaryWriter(File.OpenWrite("output/ATTNATIONWEG_ALL.DBF")))
            using (var nationalDbaseWriter = new DbaseBinaryWriter(nationalDbaseHeader, nationalDbaseFile))
            {
                nationalDbaseWriter.Write(nationalRoadRecord);
            }

            // numbered road
            var numberedRoadRecord = new NumberedRoadChangeDbaseRecord { GW_OIDN = { Value = 1 },
                WS_OIDN = { Value = 1 },
                IDENT8 = { Value = NumberedRoadNumber.Parse("A0001231").ToString() },
                RICHTING = { Value = (short) RoadSegmentNumberedRoadDirection.Backward.Translation.Identifier },
                VOLGNUMMER = { Value = 1 },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var numberedDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new NumberedRoadChangeDbaseSchema());

            using (var numberedDbaseFile = new BinaryWriter(File.OpenWrite("output/ATTGENUMWEG_ALL.DBF")))
            using (var numberedDbaseWriter = new DbaseBinaryWriter(numberedDbaseHeader, numberedDbaseFile))
            {
                numberedDbaseWriter.Write(numberedRoadRecord);
            }

            // grade separated junction
            var gradeSeparatedJunctionRecord = new GradeSeparatedJunctionChangeDbaseRecord { OK_OIDN = { Value = 1 },
                BO_WS_OIDN = { Value = 1 },
                ON_WS_OIDN = { Value = 1 },
                TYPE = { Value = (short) GradeSeparatedJunctionType.Unknown.Translation.Identifier },
                RECORDTYPE = { Value = (short)RecordType.Added.Translation.Identifier }
            };

            var gradeSeparatedJunctionDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                new GradeSeparatedJunctionChangeDbaseSchema());

            using (var gradeSeparatedJunctionDbaseFile = new BinaryWriter(File.OpenWrite("output/RLTOGKRUISING_ALL.DBF")))
            using (var gradeSeparatedJunctionDbaseWriter = new DbaseBinaryWriter(gradeSeparatedJunctionDbaseHeader, gradeSeparatedJunctionDbaseFile))
            {
                gradeSeparatedJunctionDbaseWriter.Write(gradeSeparatedJunctionRecord);
            }


            // transaction zone
            var transactionZoneRecord = new TransactionZoneDbaseRecord { ORG = { Value = "11053" },
                OPERATOR = { Value = "Yves Reynhout" },
                TYPE = { Value = 1 },
                SOURCEID = { Value = 1 },
                BESCHRIJV = { Value = "Nieuwe wijk" },
                APPLICATIE = { Value = "Wegenregister BLL" }
            };

            var transactionZoneDbaseHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                TransactionZoneDbaseRecord.Schema);

            using (var transactionZoneDbaseFile = new BinaryWriter(File.OpenWrite("output/TRANSACTIEZONE.DBF")))
            using (var transactionZoneDbaseWriter = new DbaseBinaryWriter(transactionZoneDbaseHeader, transactionZoneDbaseFile))
            {
                transactionZoneDbaseWriter.Write(transactionZoneRecord);
            }

            var suffix =
                DateTimeOffset.UtcNow.ToString("yyyyMMdd") + "-" +
                Convert.ToInt32(DateTimeOffset.UtcNow.TimeOfDay.TotalSeconds);
            using (var archiveStream = File.OpenWrite($"output/oplading-{suffix}.zip"))
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.EnumerateFiles("output"))
                {
                    if (file.ToLowerInvariant().EndsWith(".dbf")
                        || file.ToLowerInvariant().EndsWith(".shp")
                        || file.ToLowerInvariant().EndsWith(".shx"))
                    {
                        var entry = archive.CreateEntry(Path.GetFileName(file));
                        using var fileStream = File.OpenRead(file);
                        using var entryStream = entry.Open();
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
        }
    }
}
