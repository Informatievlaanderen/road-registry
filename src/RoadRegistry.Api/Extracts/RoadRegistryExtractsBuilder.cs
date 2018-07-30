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
            var fileData = new ShapeFilesData();
            var number = RecordNumber.Initial;
            var offset = ShapeRecord.InitialOffset;
            foreach (var shapeRecordBinary in roadSegments)
            {
                fileData.DbaseRecords.Add(shapeRecordBinary.DbaseRecord);

                using (var stream = new MemoryStream(shapeRecordBinary.ShapeRecordContent))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        var content = PolyLineMShapeContent.Read(reader);
                        if (content is PolyLineMShapeContent shapeContent)
                            fileData.Envelope.ExpandToInclude(shapeContent.Shape.EnvelopeInternal);

                        if (content is PolyLineMShapeContent || content is NullShapeContent)
                        {
                            var shapeRecord = content.RecordAs(number);
                            fileData.Shapes.Add(shapeRecord);

                            var indexRecord = shapeRecord.IndexAt(offset);
                            fileData.ShapeIndexes.Add(indexRecord);

                            number = number.Next();
                            offset = offset.Plus(shapeRecord.Length);
                        }
                    }
                }
            }

            return CreateShapeFiles<RoadSegmentDbaseRecord>(
                "Wegsegment",
                ShapeType.PolyLineM,
                fileData
            );
        }

        private class ShapeFilesData
        {
            public ShapeFilesData()
            {
                DbaseRecords = new List<byte[]>();
                Shapes = new List<ShapeRecord>();
                ShapeIndexes = new List<ShapeIndexRecord>();
                Envelope = new Envelope();
            }

            public IList<byte[]> DbaseRecords { get;  }
            public IList<ShapeRecord> Shapes { get;  }
            public IList<ShapeIndexRecord> ShapeIndexes { get; }
            public Envelope Envelope { get; }
        }

        private static IEnumerable<ExtractFile> CreateShapeFiles<TDbaseRecord>(
            string roadSegmentsFileName,
            ShapeType shapeType,
            ShapeFilesData data
        )
            where TDbaseRecord : DbaseRecord, new()
        {

            var dbfFile = CreateDbfFile<TDbaseRecord>(
                roadSegmentsFileName,
                new RoadSegmentDbaseSchema(),
                data.DbaseRecords
            );

            var boundingBox = new BoundingBox3D(data.Envelope.MinX, data.Envelope.MinY, data.Envelope.MaxX, data.Envelope.MaxY, 0, 0, 0, 0);

            var shpFileLength = data.Shapes.Aggregate(
                new WordLength(50),
                (length, record) => length.Plus(record.Length)
            );
            var shpFile = new ShpFile(
                roadSegmentsFileName,
                new ShapeFileHeader(
                    shpFileLength,
                    shapeType,
                    boundingBox
                )
            );
            shpFile.Write(data.Shapes);

            var shxFile = new ShxFile(
                roadSegmentsFileName,
                new ShapeFileHeader(
                    new WordLength(50 + 4 * data.ShapeIndexes.Count),
                    shapeType,
                    boundingBox
                )
            );
            shxFile.Write(data.ShapeIndexes);

            return new[]
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
                roadSegmentDynamicLaneAttributes.Select(record => record.DbaseRecord)
            );
        }

        public ExtractFile CreateRoadSegmentDynamicWidtAttributesFile(IReadOnlyCollection<RoadSegmentDynamicWidthAttributeRecord> roadSegmentDynamicWidthAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicWidthAttributeDbaseRecord>(
                "AttWegbreedte",
                new RoadSegmentDynamicWidthAttributeDbaseSchema(),
                roadSegmentDynamicWidthAttributes.Select(record => record.DbaseRecord)
            );
        }

        public ExtractFile CreateRoadSegmentDynamicHardeningAttributesFile(IReadOnlyCollection<RoadSegmentDynamicHardeningAttributeRecord> roadSegmentDynamicHardeningAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicHardeningAttributeDbaseRecord>(
                "AttWegverharding",
                new RoadSegmentDynamicHardeningAttributeDbaseSchema(),
                roadSegmentDynamicHardeningAttributes.Select(record => record.DbaseRecord)
            );
        }

        public ExtractFile CreateRoadSegmentNationalRoadAttributesFile(IReadOnlyCollection<RoadSegmentNationalRoadAttributeRecord> roadSegmentNationalRoadAttributeRecords)
        {
            return CreateDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(
                "AttNationweg",
                new RoadSegmentNationalRoadAttributeDbaseSchema(),
                roadSegmentNationalRoadAttributeRecords.Select(record => record.DbaseRecord)
            );
        }

        public ExtractFile CreateRoadSegmentEuropeanRoadAttributesFile(IReadOnlyCollection<RoadSegmentEuropeanRoadAttributeRecord> roadSegmentEuropeanRoadAttributeRecords)
        {
            return CreateDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                "AttEuropweg",
                new RoadSegmentEuropeanRoadAttributeDbaseSchema(),
                roadSegmentEuropeanRoadAttributeRecords.Select(record => record.DbaseRecord)
            );
        }

        public ExtractFile CreateRoadSegmentNumberedRoadAttributesFile(IReadOnlyCollection<RoadSegmentNumberedRoadAttributeRecord> roadSegmentNumberedRoadAttributeRecords)
        {
            return CreateDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                "AttGenumweg",
                new RoadSegmentNumberedRoadAttributeDbaseSchema(),
                roadSegmentNumberedRoadAttributeRecords.Select(record => record.DbaseRecord)
            );
        }

        public ExtractFile CreateGradeSeperatedJuctionsFile(IReadOnlyCollection<GradeSeparatedJunctionRecord> gradeSeparatedJunctionRecords)
        {
            return CreateDbfFile<GradeSeparatedJunctionDbaseRecord>(
                "RltOgkruising",
                new GradeSeparatedJunctionDbaseSchema(),
                gradeSeparatedJunctionRecords.Select(record => record.DbaseRecord)
            );

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

        private static ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, IEnumerable<byte[]> records)
            where TDbaseRecord : DbaseRecord, new()
        {
            return CreateDbfFile<TDbaseRecord>(fileName, schema, records.ToArray());
        }

        private static ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, IReadOnlyCollection<byte[]> records)
            where TDbaseRecord : DbaseRecord, new()
        {
            var dbfFile = CreateEmptyDbfFile<TDbaseRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count)
            );
            dbfFile.WriteBytesAs<TDbaseRecord>(records);

            return dbfFile;
        }

        private static ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, IReadOnlyCollection<TDbaseRecord> records, DbaseSchema schema)
            where TDbaseRecord : DbaseRecord
        {
            var dbfFile = CreateEmptyDbfFile<TDbaseRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count)
            );
            dbfFile.Write(records);

            return dbfFile;
        }

        private static DbfFile<TDbaseRecord> CreateEmptyDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, DbaseRecordCount recordCount)
            where TDbaseRecord : DbaseRecord
        {
            return new DbfFile<TDbaseRecord>(
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
