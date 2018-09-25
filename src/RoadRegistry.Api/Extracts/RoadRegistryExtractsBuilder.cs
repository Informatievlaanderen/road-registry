namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ExtractFiles;
    using GeoAPI.Geometries;
    using Projections;
    using Shaperon;

    public class RoadRegistryExtractsBuilder
    {
        private readonly Action<ExtractFile> _showWriteFilesContentMessage;

        public RoadRegistryExtractsBuilder(Action<string> showMessage)
        {
            _showWriteFilesContentMessage = file => showMessage?.Invoke($"Write file {file.Name}");
        }

        public async Task<ExtractFile> CreateOrganizationsFileAsync(Task<IReadOnlyCollection<OrganizationRecord>> organisations)
        {
            return CreateDbfFile<OrganizationDbaseRecord>(
                "LstOrg",
                new OrganizationDbaseSchema(),
                (await organisations).Select(org => org.DbaseRecord)
            );
        }

        public async Task<IEnumerable<ExtractFile>> CreateRoadSegmentsFilesAsync(Task<IReadOnlyCollection<RoadSegmentRecord>> roadSegments)
        {
            var shapeData = (await roadSegments).Select(segment =>
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

        public async Task<IEnumerable<ExtractFile>> CreateRoadNodesFilesAsync(Task<IReadOnlyCollection<RoadNodeRecord>> roadNodes)
        {
            var shapeData = (await roadNodes).Select(node =>
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

        public async Task<IEnumerable<ExtractFile>> CreateReferencePointsFilesAsync(Task<IReadOnlyCollection<RoadReferencePointRecord>> referencePointRecords)
        {
            return CreateShapeFiles<RoadReferencePointDbaseRecord, PointShapeContent>(
                "Refpunt",
                ShapeType.Point,
                new RoadReferencePointDbaseSchema(),
                (await referencePointRecords).Select(referencePoint => new ShapeData{ DbaseRecord = referencePoint.DbaseRecord, Shape = referencePoint.ShapeRecordContent}),
                PointShapeContent.Read,
                content => content.Shape.EnvelopeInternal
            );
        }

        private class ShapeData
        {
            public byte[] DbaseRecord { get; set; }
            public byte[] Shape { get; set; }
        }

        private IEnumerable<ExtractFile> CreateShapeFiles<TDbaseRecord, TShape>(
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
            _showWriteFilesContentMessage(shpFile);
            shpFile.Write(data.ShapeRecords);

            var shxFile = new ShxFile(
                fileName,
                new ShapeFileHeader(
                    new WordLength(ShapeRecord.InitialOffset + (4 * data.ShapeIndexRecords.Count)),
                    shapeType,
                    boundingBox
                )
            );
            _showWriteFilesContentMessage(shxFile);
            shxFile.Write(data.ShapeIndexRecords);

            return new[]
            {
                dbfFile,
                shpFile,
                shxFile
            };
        }

        public async Task<ExtractFile> CreateRoadSegmentDynamicLaneAttributesFileAsync(Task<IReadOnlyCollection<RoadSegmentDynamicLaneAttributeRecord>> roadSegmentDynamicLaneAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicLaneAttributeDbaseRecord>(
                "AttRijstroken",
                new RoadSegmentDynamicLaneAttributeDbaseSchema(),
                (await roadSegmentDynamicLaneAttributes).Select(record => record.DbaseRecord)
            );
        }

        public async Task<ExtractFile> CreateRoadSegmentDynamicWidtAttributesFileAsync(Task<IReadOnlyCollection<RoadSegmentDynamicWidthAttributeRecord>> roadSegmentDynamicWidthAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicWidthAttributeDbaseRecord>(
                "AttWegbreedte",
                new RoadSegmentDynamicWidthAttributeDbaseSchema(),
                (await roadSegmentDynamicWidthAttributes).Select(record => record.DbaseRecord)
            );
        }

        public async Task<ExtractFile> CreateRoadSegmentDynamicHardeningAttributesFileAsync(Task<IReadOnlyCollection<RoadSegmentDynamicHardeningAttributeRecord>> roadSegmentDynamicHardeningAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicHardeningAttributeDbaseRecord>(
                "AttWegverharding",
                new RoadSegmentDynamicHardeningAttributeDbaseSchema(),
                (await roadSegmentDynamicHardeningAttributes).Select(record => record.DbaseRecord)
            );
        }

        public async Task<ExtractFile> CreateRoadSegmentNationalRoadAttributesFileAsync(Task<IReadOnlyCollection<RoadSegmentNationalRoadAttributeRecord>> roadSegmentNationalRoadAttributeRecords)
        {
            return CreateDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(
                "AttNationweg",
                new RoadSegmentNationalRoadAttributeDbaseSchema(),
                (await roadSegmentNationalRoadAttributeRecords).Select(record => record.DbaseRecord)
            );
        }

        public async Task<ExtractFile> CreateRoadSegmentEuropeanRoadAttributesFileAsync(Task<IReadOnlyCollection<RoadSegmentEuropeanRoadAttributeRecord>> roadSegmentEuropeanRoadAttributeRecords)
        {
            return CreateDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                "AttEuropweg",
                new RoadSegmentEuropeanRoadAttributeDbaseSchema(),
                (await roadSegmentEuropeanRoadAttributeRecords).Select(record => record.DbaseRecord)
            );
        }

        public async Task<ExtractFile> CreateRoadSegmentNumberedRoadAttributesFileAsync(Task<IReadOnlyCollection<RoadSegmentNumberedRoadAttributeRecord>> roadSegmentNumberedRoadAttributeRecords)
        {
            return CreateDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                "AttGenumweg",
                new RoadSegmentNumberedRoadAttributeDbaseSchema(),
                (await roadSegmentNumberedRoadAttributeRecords).Select(record => record.DbaseRecord)
            );
        }

        public async Task<ExtractFile> CreateGradeSeperatedJunctionsFileAsync(Task<IReadOnlyCollection<GradeSeparatedJunctionRecord>> gradeSeparatedJunctionRecords)
        {
            return CreateDbfFile<GradeSeparatedJunctionDbaseRecord>(
                "RltOgkruising",
                new GradeSeparatedJunctionDbaseSchema(),
                (await gradeSeparatedJunctionRecords).Select(record => record.DbaseRecord)
            );

        }

        public Task<ExtractFile> CreateRoadNodeTypesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegknoopLktType",
                TypeReferences.RoadNodeTypes,
                new RoadNodeTypeDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateRoadSegmentCategoriesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegsegmentLktWegcat",
                TypeReferences.RoadSegmentCategories,
                new RoadSegmentCategoryDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateRoadSegmentGeometryDrawMethodsFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegsegmentLktMethode",
                TypeReferences.RoadSegmentGeometryDrawMethods,
                new RoadSegmentGeometryDrawMethodDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateRoadSegmentStatusesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegsegmentLktStatus",
                TypeReferences.RoadSegmentStatuses,
                new RoadSegmentStatusDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateRoadSegmentAccessRestrictionsFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegsegmentLktTgbep",
                TypeReferences.RoadSegmentAccessRestrictions,
                new RoadSegmentAccessRestrictionDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateReferencePointTypesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "RefpuntLktType",
                TypeReferences.ReferencePointTypes,
                new ReferencePointTypeDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateRoadSegmentMorphologiesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegsegmentLktMorf",
                TypeReferences.RoadSegmentMorphologies,
                new RoadSegmentMorphologyDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateGradeSeperatedJunctionTypesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "OgkruisingLktType",
                TypeReferences.GradeSeparatedJunctionTypes,
                new GradeSeparatedJunctionTypeDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateNumberedRoadSegmentDirectionsFile()
        {
            return Task.Run(() => CreateDbfFile(
                "GenumwegLktRichting",
                TypeReferences.NumberedRoadSegmentDirections,
                new NumberedRoadSegmentDirectionDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateHardeningTypesFile()
        {
            return Task.Run(() => CreateDbfFile(
                "WegverhardLktType",
                TypeReferences.HardeningTypes,
                new HardeningTypeDbaseSchema()
            ));
        }

        public Task<ExtractFile> CreateLaneDirectionsFile()
        {
            return Task.Run(() => CreateDbfFile(
                "RijstrokenLktRichting",
                TypeReferences.LaneDirections,
                new LaneDirectionDbaseSchema()
            ));
        }

        private ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, IEnumerable<byte[]> records)
            where TDbaseRecord : DbaseRecord, new()
        {
            return CreateDbfFile<TDbaseRecord>(fileName, schema, records.ToArray());
        }

        private ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, IReadOnlyCollection<byte[]> records)
            where TDbaseRecord : DbaseRecord, new()
        {
            var dbfFile = CreateEmptyDbfFile<TDbaseRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count)
            );
            _showWriteFilesContentMessage(dbfFile);
            dbfFile.WriteBytesAs<TDbaseRecord>(records);

            return dbfFile;
        }

        private ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, IReadOnlyCollection<TDbaseRecord> records, DbaseSchema schema)
            where TDbaseRecord : DbaseRecord
        {
            var dbfFile = CreateEmptyDbfFile<TDbaseRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count)
            );
            _showWriteFilesContentMessage(dbfFile);
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
