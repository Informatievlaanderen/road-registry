namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ExtractFiles;
    using GeoAPI.Geometries;
    using Projections;
    using Shaperon;

    public class RoadRegistryExtractsBuilder
    {
        public RoadRegistryExtractsBuilder()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public IEnumerable<ExtractFile> CreateRoadSegmentFiles(IReadOnlyCollection<RoadSegmentRecord> roadSegments)
        {
            const string roadSegmentsFileName = "Wegsegment";



            var dbfFile = CreateEmptyDbfFile<RoadSegmentDbaseRecord>(
                roadSegmentsFileName,
                new RoadSegmentDbaseSchema(),
                new DbaseRecordCount(roadSegments.Count)
            );
            var shpFileLength = new WordLength(50);
            var shpRecords = new List<ShapeRecord>();
            var shxRecords = new List<ShapeIndexRecord>();
            var envelope = new Envelope();
            var number = RecordNumber.Initial;
            var offset = ShapeRecord.InitialOffset;
            foreach (var segment in roadSegments)
            {
                dbfFile.WriteBytesAs<RoadSegmentDbaseRecord>(segment.DbaseRecord);

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

            var shpFile = new ShpFile(
                roadSegmentsFileName,
                new ShapeFileHeader(
                    shpFileLength,
                    ShapeType.PolyLineM,
                    new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, 0, 0)
                )
            );
            shpFile.Write(shpRecords);

            var shxFile = new ShxFile(
                roadSegmentsFileName,
                new ShapeFileHeader(
                    new WordLength(50 + 4 * shxRecords.Count),
                    ShapeType.PolyLineM,
                    new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, 0, 0)
                )
            );
            shxFile.Write(shxRecords);

            return new ExtractFile[]
            {
                dbfFile,
                shpFile,
                shxFile
            };
        }

        public ExtractFile CreateRoadSegmentDynamicLaneAttributeFile(IReadOnlyCollection<RoadSegmentDynamicLaneAttributeRecord> roadSegmentDynamicLaneAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicLaneAttributeDbaseRecord>(
                "AttRijstroken",
                new RoadSegmentDynamicLaneAttributeDbaseSchema(),
                roadSegmentDynamicLaneAttributes.Select(record => record.DbaseRecord).ToArray());
        }

        public ExtractFile CreateLaneDirectionFile()
        {
            return CreateDbfFile(
                "RijstrokenLktRichting",
                TypeReferences.LaneDirections,
                new LaneDirectionDbaseSchema()
            );
        }

        private static ExtractFile CreateDbfFile<TRecord>(string fileName, DbaseSchema schema, IReadOnlyCollection<byte[]> records)
            where TRecord : DbaseRecord, new()
        {
            var dbfFile = CreateEmptyDbfFile<TRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count)
            );
            dbfFile.WriteBytesAs<TRecord>(records);

            return dbfFile;
        }

        private static ExtractFile CreateDbfFile<TRecord>(string fileName, IReadOnlyCollection<TRecord> records, DbaseSchema schema)
            where TRecord : DbaseRecord
        {
            var dbfFile = CreateEmptyDbfFile<TRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count)
            );
            dbfFile.Write(records);

            return dbfFile;
        }

        private static DbfFile<TRecord> CreateEmptyDbfFile<TRecord>(string fileName, DbaseSchema schema, DbaseRecordCount recordCount)
            where TRecord : DbaseRecord
        {
            return new DbfFile<TRecord>(
                fileName,
                new DbaseFileHeader(
                    DateTime.Now,
                    DbaseCodePage.WindowsANSI,
                    recordCount,
                    schema
                )
            );
        }
    }
}
