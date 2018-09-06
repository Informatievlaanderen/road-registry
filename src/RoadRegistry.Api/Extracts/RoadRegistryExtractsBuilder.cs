namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ExtractFiles;
    using GeoAPI.Geometries;
    using Projections;
    using Shaperon;

    public class RoadRegistryExtractsBuilder
    {
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
            var shapeData = roadSegments.Select(segment =>
                new ShapeData
                {
                    DbaseRecord = segment.DbaseRecord,
                    Shape = segment.ShapeRecordContent
                });

            return CreateShapeFiles<RoadSegmentDbaseRecord, PolyLineMShapeContent>(
                "Wegsegment",
                ShapeType.PolyLineM,
                new RoadSegmentDbaseSchema(),
                shapeData,
                PolyLineMShapeContent.Read,
                content => content.Shape.EnvelopeInternal
            );
        }

        public IEnumerable<ExtractFile> CreateRoadNodesFiles(IReadOnlyCollection<RoadNodeRecord> roadNodes)
        {
            var shapeData = roadNodes.Select(node =>
                    new ShapeData
                    {
                        DbaseRecord = node.DbaseRecord,
                        Shape =  node.ShapeRecordContent
                    });

            return CreateShapeFiles<RoadNodeDbaseRecord, PointShapeContent>(
                "Wegknoop",
                ShapeType.Point,
                new RoadNodeDbaseSchema(),
                shapeData,
                PointShapeContent.Read,
                content => content.Shape.EnvelopeInternal
            );
        }

        public IEnumerable<ExtractFile> CreateReferencePointsFiles(IReadOnlyCollection<RoadReferencePointRecord> referencePointRecords)
        {
            return CreateShapeFiles<RoadReferencePointDbaseRecord, PointShapeContent>(
                "Refpunt",
                ShapeType.Point,
                new RoadReferencePointDbaseSchema(),
                referencePointRecords.Select(referencePoint => new ShapeData{ DbaseRecord = referencePoint.DbaseRecord, Shape = referencePoint.ShapeRecordContent}),
                PointShapeContent.Read,
                content => content.Shape.EnvelopeInternal
            );
        }

        private class ShapeData
        {
            public byte[] DbaseRecord { get; set; }
            public byte[] Shape { get; set; }
        }

        private static IEnumerable<ExtractFile> CreateShapeFiles<TDbaseRecord, TShape>(
            string fileName,
            ShapeType shapeType,
            DbaseSchema schema,
            IEnumerable<ShapeData> shapeDataRecords,
            Func<BinaryReader, ShapeContent> readShape,
            Func<TShape, Envelope> getEnvelope
        )
            where TDbaseRecord : DbaseRecord, new()
            where TShape : ShapeContent
        {
            var data = new
            {
                DbaseRecords = new List<byte[]>(),
                ShapeRecords = new List<ShapeRecord>(),
                ShapeIndexRecords = new List<ShapeIndexRecord>()
            };
            var envelope = new Envelope();
            var number = RecordNumber.Initial;
            var offset = ShapeRecord.InitialOffset;
            foreach (var shapeData in shapeDataRecords)
            {
                data.DbaseRecords.Add(shapeData.DbaseRecord);
                using (var stream = new MemoryStream(shapeData.Shape))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        var content = readShape(reader);
                        if (typeof(TShape) != typeof(NullShapeContent) && content is TShape shapeContent)
                            envelope.ExpandToInclude(getEnvelope(shapeContent));

                        if (content is TShape || content is NullShapeContent)
                        {
                            var shapeRecord = content.RecordAs(number);
                            data.ShapeRecords.Add(shapeRecord);

                            var indexRecord = shapeRecord.IndexAt(offset);
                            data.ShapeIndexRecords.Add(indexRecord);

                            number = number.Next();
                            offset = offset.Plus(shapeRecord.Length);
                        }
                    }
                }
            }

            var dbfFile = CreateDbfFile<TDbaseRecord>(
                fileName,
                schema,
                data.DbaseRecords
            );

            var mMin = shapeType == ShapeType.PolyLineM ? double.NegativeInfinity : 0;
            var mMax = shapeType == ShapeType.PolyLineM ? double.PositiveInfinity : 0;
            var boundingBox = new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, mMin, mMax);

            var shpFileLength = data.ShapeRecords.Aggregate(
                new WordLength(ShapeRecord.InitialOffset),
                (length, record) => length.Plus(record.Length)
            );
            var shpFile = new ShpFile(
                fileName,
                new ShapeFileHeader(
                    shpFileLength,
                    shapeType,
                    boundingBox
                )
            );
            shpFile.Write(data.ShapeRecords);

            var shxFile = new ShxFile(
                fileName,
                new ShapeFileHeader(
                    new WordLength(ShapeRecord.InitialOffset + (4 * data.ShapeIndexRecords.Count)),
                    shapeType,
                    boundingBox
                )
            );
            shxFile.Write(data.ShapeIndexRecords);

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

        public ExtractFile CreateGradeSeperatedJunctionsFile(IReadOnlyCollection<GradeSeparatedJunctionRecord> gradeSeparatedJunctionRecords)
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

        public ExtractFile CreateRoadSegmentGeometryDrawMethodsFile()
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

        public ExtractFile CreateGradeSeperatedJunctionTypesFile()
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
                    DbaseCodePage.Western_European_ANSI,
                    recordCount,
                    schema
                )
            );
        }
    }
}
