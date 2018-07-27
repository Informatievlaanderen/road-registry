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

        public ExtractFile CreateOrganizationsFile(IReadOnlyCollection<OrganizationRecord> organisations)
        {
            return CreateDbfFile<OrganizationDbaseRecord>(
                "LstOrg",
                new OrganizationDbaseSchema(),
                organisations.Select(org => org.DbaseRecord)
            );
        }

        public IEnumerable<ExtractFile> CreateRoadSegmentsFiles(IReadOnlyCollection<RoadSegmentRecord> roadSegments)
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

        public ExtractFile CreateRoadSegmentDynamicLaneAttributesFile(IReadOnlyCollection<RoadSegmentDynamicLaneAttributeRecord> roadSegmentDynamicLaneAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicLaneAttributeDbaseRecord>(
                "AttRijstroken",
                new RoadSegmentDynamicLaneAttributeDbaseSchema(),
                roadSegmentDynamicLaneAttributes.Select(record => record.DbaseRecord));
        }

        public ExtractFile CreateRoadNodeTypesFile()
        {
            return CreateDbfFile(
                "WegknoopLktType",
                TypeReferences.RoadNodeTypes,
                new RoadNodeTypeDbaseSchema()
            );
        }

        public ExtractFile CreateRoadSegmentCategoriesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktWegcat",
                TypeReferences.RoadSegmentCategories,
                new RoadSegmentCategoryDbaseSchema()
            );
        }

        public ExtractFile CreateRoadSegmentGeometryDrawMethdsFile()
        {
            return CreateDbfFile(
                "WegsegmentLktMethode",
                TypeReferences.RoadSegmentGeometryDrawMethods,
                new RoadSegmentGeometryDrawMethodDbaseSchema()
            );
        }

        public ExtractFile CreateRoadSegmentStatusesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktStatus",
                TypeReferences.RoadSegmentStatuses,
                new RoadSegmentStatusDbaseSchema()
            );
        }

        public ExtractFile CreateRoadSegmentAccessRestrictionsFile()
        {
            return CreateDbfFile(
                "WegsegmentLktTgbep",
                TypeReferences.RoadSegmentAccessRestrictions,
                new RoadSegmentAccessRestrictionDbaseSchema()
            );
        }

        public ExtractFile CreateReferencePointTypesFile()
        {
            return CreateDbfFile(
                "RefpuntLktType",
                TypeReferences.ReferencePointTypes,
                new ReferencePointTypeDbaseSchema()
            );
        }

        public ExtractFile CreateRoadSegmentMorphologiesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktMorf",
                TypeReferences.RoadSegmentMorphologies,
                new RoadSegmentMorphologyDbaseSchema()
            );
        }

        public ExtractFile CreateGradeSeperatedJuctionTypesFile()
        {
            return CreateDbfFile(
                "OgkruisingLktType",
                TypeReferences.GradeSeparatedJunctionTypes,
                new GradeSeparatedJunctionTypeDbaseSchema()
            );
        }

        public ExtractFile CreateNumberedRoadSegmentDirectionsFile()
        {
            return CreateDbfFile(
                "GenumwegLktRichting",
                TypeReferences.NumberedRoadSegmentDirections,
                new NumberedRoadSegmentDirectionDbaseSchema()
            );
        }

        public ExtractFile CreateHardeningTypesFile()
        {
            return CreateDbfFile(
                "WegverhardLktType",
                TypeReferences.HardeningTypes,
                new HardeningTypeDbaseSchema()
            );
        }

        public ExtractFile CreateLaneDirectionsFile()
        {
            return CreateDbfFile(
                "RijstrokenLktRichting",
                TypeReferences.LaneDirections,
                new LaneDirectionDbaseSchema()
            );
        }

        private static ExtractFile CreateDbfFile<TRecord>(string fileName, DbaseSchema schema, IEnumerable<byte[]> records)
            where TRecord : DbaseRecord, new()
        {
            return CreateDbfFile<TRecord>(fileName, schema, records.ToArray());
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
