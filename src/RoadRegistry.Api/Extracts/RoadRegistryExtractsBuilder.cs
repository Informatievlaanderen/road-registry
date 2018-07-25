namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using GeoAPI.Geometries;
    using Projections;
    using Shaperon;

    public class RoadRegistryExtractsBuilder
    {
        public RoadRegistryExtractsBuilder()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public IEnumerable<RoadRegistryExtractFile> CreateRoadSegmentFiles(IReadOnlyCollection<RoadSegmentRecord> roadSegments)
        {
            // TODO: change to name currently used in dump
            const string roadSegmentsFileName = "roadsegment";

            // TODO: ADD USING STATEMENTS FOR STREAMS IF POSSIBLE
            var encoding = Encoding.GetEncoding(1252);
            var dbfFile = new MemoryStream();
            var shpFile = new MemoryStream();
            var shxFile = new MemoryStream();
            var shxWriter = new BinaryWriter(shxFile, Encoding.ASCII);
            var shpWriter = new BinaryWriter(shpFile, Encoding.ASCII);
            var dbfWriter = new BinaryWriter(dbfFile, encoding);
            {
                var dbfHeader = new DbaseFileHeader(
                    DateTime.Now,
                    DbaseCodePage.WindowsANSI,
                    new DbaseRecordCount(roadSegments.Count),
                    new RoadNodeDbaseSchema()
                );
                dbfHeader.Write(dbfWriter);
                var shpFileLength = new WordLength(50);
                var dbfRecord = new RoadSegmentDbaseRecord();
                var shpRecords = new List<ShapeRecord>();
                var shxRecords = new List<ShapeIndexRecord>();
                var envelope = new Envelope();
                var number = RecordNumber.Initial;
                var offset = ShapeRecord.InitialOffset;
                foreach (var segment in roadSegments)
                {
                    dbfRecord.FromBytes(segment.DbaseRecord, encoding);
                    dbfRecord.Write(dbfWriter);

                    using (var stream = new MemoryStream(segment.ShapeRecordContent))
                    {
                        using (var reader = new BinaryReader(stream))
                        {
                            switch (PolyLineMShapeContent.Read(reader))
                            {
                                case PolyLineMShapeContent content:
                                    envelope.ExpandToInclude(content.Shape.EnvelopeInternal);
                                    var shpRecord1 = content.RecordAs(number);
                                    shpFileLength = shpFileLength.Plus(shpRecord1.Length);
                                    var shxRecord1 = shpRecord1.IndexAt(offset);
                                    shpRecords.Add(shpRecord1);
                                    shxRecords.Add(shxRecord1);
                                    offset = offset.Plus(shpRecord1.Length);
                                    number = number.Next();
                                    break;
                                case NullShapeContent content:
                                    var shpRecord2 = content.RecordAs(number);
                                    shpFileLength = shpFileLength.Plus(shpRecord2.Length);
                                    var shxRecord2 = shpRecord2.IndexAt(offset);
                                    shpRecords.Add(shpRecord2);
                                    shxRecords.Add(shxRecord2);
                                    offset = offset.Plus(shpRecord2.Length);
                                    number = number.Next();
                                    break;
                            }
                        }
                    }
                }

                dbfWriter.Write(DbaseRecord.EndOfFile);

                var shpHeader = new ShapeFileHeader(
                    shpFileLength,
                    ShapeType.PolyLineM,
                    new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, 0, 0)
                );
                shpHeader.Write(shpWriter);
                foreach (var shpRecord in shpRecords)
                    shpRecord.Write(shpWriter);
                shpWriter.Flush();

                var shxHeader = new ShapeFileHeader(
                    new WordLength(50 + 4 * shxRecords.Count),
                    ShapeType.PolyLineM,
                    new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, 0, 0)
                );
                shxHeader.Write(shxWriter);
                foreach (var shxRecord in shxRecords)
                    shxRecord.Write(shxWriter);
                shxWriter.Flush();
                
                return new[]
                {
                    new RoadRegistryExtractFile($"{roadSegmentsFileName}.dbf", dbfFile),
                    new RoadRegistryExtractFile($"{roadSegmentsFileName}.shp", shpFile),
                    new RoadRegistryExtractFile($"{roadSegmentsFileName}.shx", shxFile),
                };
            }
        }
    }
}
