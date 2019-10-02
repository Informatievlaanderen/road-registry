using System;

namespace RoadRegistry.BackOffice.Simulator
{
    using System.IO;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Model;
    using Translation;
    using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("output");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var node1Record = new RoadNodeChangeDbaseRecord();
            node1Record.TYPE.Value = (short)RoadNodeType.FakeNode.Translation.Identifier;
            node1Record.RECORDTYPE.Value = RecordTypes.Added;
            node1Record.WEGKNOOPID.Value = 1;
            var node1Shape = new PointShapeContent(new Point(0.0, 0.0));

            var node2Record = new RoadNodeChangeDbaseRecord();
            node2Record.TYPE.Value = (short)RoadNodeType.FakeNode.Translation.Identifier;
            node2Record.RECORDTYPE.Value = RecordTypes.Added;
            node2Record.WEGKNOOPID.Value = 2;
            var node2Shape = new PointShapeContent(new Point(0.0, 1.0));

            var segment1Record = new RoadSegmentChangeDbaseRecord();
            segment1Record.WS_OIDN.Value = 1;
            segment1Record.B_WK_OIDN.Value = 1;
            segment1Record.E_WK_OIDN.Value = 2;
            segment1Record.TGBEP.Value = (short) RoadSegmentAccessRestriction.PublicRoad.Translation.Identifier;
            segment1Record.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier;
            segment1Record.WEGCAT.Value = RoadSegmentCategory.SecondaryRoad.Translation.Identifier;
            segment1Record.METHODE.Value = (short)RoadSegmentGeometryDrawMethod.Measured.Translation.Identifier;
            segment1Record.MORFOLOGIE.Value = (short)RoadSegmentMorphology.Motorway.Translation.Identifier;
            segment1Record.LSTRNMID.Value = 123;
            segment1Record.RSTRNMID.Value = 456;
            segment1Record.BEHEERDER.Value = "71002";
            segment1Record.RECORDTYPE.Value = RecordTypes.Added;
            var segmentShape = new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(new NetTopologySuite.Geometries.MultiLineString(new NetTopologySuite.Geometries.LineString[]
            {
                new NetTopologySuite.Geometries.LineString(new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(new[]
                {
                    new NetTopologySuite.Geometries.Coordinate(0.0, 0.0),
                    new NetTopologySuite.Geometries.Coordinate(0.0, 1.0)
                }), GeometryConfiguration.GeometryFactory)
            })));

            var laneRecord = new RoadSegmentLaneChangeDbaseRecord();
            laneRecord.RS_OIDN.Value = 1;
            laneRecord.WS_OIDN.Value = 1;
            laneRecord.RICHTING.Value = (short)RoadSegmentLaneDirection.Independent.Translation.Identifier;
            laneRecord.AANTAL.Value = 2;
            laneRecord.VANPOSITIE.Value = 0.0;
            laneRecord.TOTPOSITIE.Value = 1.0;
            laneRecord.RECORDTYPE.Value = RecordTypes.Added;

            var surfaceRecord = new RoadSegmentSurfaceChangeDbaseRecord();
            surfaceRecord.WV_OIDN.Value = 1;
            surfaceRecord.WS_OIDN.Value = 1;
            surfaceRecord.TYPE.Value = (short) RoadSegmentSurfaceType.SolidSurface.Translation.Identifier;
            surfaceRecord.VANPOSITIE.Value = 0.0;
            surfaceRecord.TOTPOSITIE.Value = 1.0;
            surfaceRecord.RECORDTYPE.Value = RecordTypes.Added;

            var widthRecord = new RoadSegmentWidthChangeDbaseRecord();
            widthRecord.WB_OIDN.Value = 1;
            widthRecord.WS_OIDN.Value = 1;
            widthRecord.BREEDTE.Value = 1;
            widthRecord.VANPOSITIE.Value = 0.0;
            widthRecord.TOTPOSITIE.Value = 1.0;
            widthRecord.RECORDTYPE.Value = RecordTypes.Added;

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
            var europeanRoadRecord = new EuropeanRoadChangeDbaseRecord();
            europeanRoadRecord.EU_OIDN.Value = 1;
            europeanRoadRecord.WS_OIDN.Value = 1;
            europeanRoadRecord.EUNUMMER.Value = EuropeanRoadNumber.E40.ToString();
            europeanRoadRecord.RECORDTYPE.Value = RecordTypes.Added;

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
            var nationalRoadRecord = new NationalRoadChangeDbaseRecord();
            nationalRoadRecord.NW_OIDN.Value = 1;
            nationalRoadRecord.WS_OIDN.Value = 1;
            nationalRoadRecord.IDENT2.Value = NationalRoadNumber.All[5].ToString();
            nationalRoadRecord.RECORDTYPE.Value = RecordTypes.Added;

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
            var numberedRoadRecord = new NumberedRoadChangeDbaseRecord();
            numberedRoadRecord.GW_OIDN.Value = 1;
            numberedRoadRecord.WS_OIDN.Value = 1;
            numberedRoadRecord.IDENT8.Value = NumberedRoadNumber.All[5].ToString();
            numberedRoadRecord.RICHTING.Value = (short) RoadSegmentNumberedRoadDirection.Backward.Translation.Identifier;
            numberedRoadRecord.VOLGNUMMER.Value = 1;
            numberedRoadRecord.RECORDTYPE.Value = RecordTypes.Added;

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
            var gradeSeparatedJunctionRecord = new GradeSeparatedJunctionChangeDbaseRecord();
            gradeSeparatedJunctionRecord.OK_OIDN.Value = 1;
            gradeSeparatedJunctionRecord.BO_WS_OIDN.Value = 1;
            gradeSeparatedJunctionRecord.ON_WS_OIDN.Value = 1;
            gradeSeparatedJunctionRecord.TYPE.Value = (short) GradeSeparatedJunctionType.Unknown.Translation.Identifier;
            gradeSeparatedJunctionRecord.RECORDTYPE.Value = RecordTypes.Added;

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
        }
    }
}
