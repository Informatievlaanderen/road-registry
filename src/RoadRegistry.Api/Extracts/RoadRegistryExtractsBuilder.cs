namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ExtractFiles;
    using GeoAPI.Geometries;
    using Projections;
    using Aiv.Vbr.Shaperon;

    public class RoadRegistryExtractsBuilder
    {
        public IEnumerable<ExtractFile> CreateRoadSegmentsFiles(ShapeContext context)
        {
            const string fileName = "Wegsegment";
            const ShapeType shapeType = ShapeType.PolyLineM;
            Func<BinaryReader, ShapeContent> readShape = PolyLineMShapeContent.Read;
            int SegmentId(RoadSegmentRecord record) => record.Id;

            // ToDo get projected record count so dbset is not executed
            var recordCount = context.RoadSegments.Count();

            yield return CreateDbfFile<RoadSegmentDbaseRecord>(
                fileName,
                new RoadSegmentDbaseSchema(),
                context
                    .RoadSegments
                    .OrderBy(SegmentId)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));

            // ToDo get projected header values
            var headerValues = CalculateShapeHeaderValues<RoadSegmentRecord, PolyLineMShapeContent>(
                shapeType,
                context.RoadSegments,
                record => record.ShapeRecordContent,
                readShape,
                content => content.Shape.EnvelopeInternal);

            yield return CreateShapeFile<PolyLineMShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadSegments
                    .OrderBy(SegmentId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                headerValues.ShapeFileLength,
                headerValues.BoundingBox);

            yield return CreateShapeIndexFile<PolyLineMShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadSegments
                    .OrderBy(SegmentId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                headerValues.ShapeIndexFileLegth,
                headerValues.BoundingBox);
        }

        public IEnumerable<ExtractFile> CreateRoadNodesFiles(ShapeContext context)
        {
            const string fileName = "Wegknoop";
            const ShapeType shapeType = ShapeType.Point;
            Func<BinaryReader, ShapeContent> readShape = PointShapeContent.Read;
            int NodeId(RoadNodeRecord record) => record.Id;

            // ToDo get projected record count so dbset is not executed
            var recordCount = context.RoadNodes.Count();

            yield return CreateDbfFile<RoadNodeDbaseRecord>(
                fileName,
                new RoadNodeDbaseSchema(),
                context
                    .RoadNodes
                    .OrderBy(NodeId)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));

            // ToDo get projected header values
            var headerValues = CalculateShapeHeaderValues<RoadNodeRecord, PointShapeContent>(
                shapeType,
                context.RoadNodes,
                record => record.ShapeRecordContent,
                readShape,
                content => content.Shape.EnvelopeInternal);

            yield return CreateShapeFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadNodes
                    .OrderBy(NodeId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                headerValues.ShapeFileLength,
                headerValues.BoundingBox);

            yield return CreateShapeIndexFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadNodes
                    .OrderBy(NodeId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                headerValues.ShapeIndexFileLegth,
                headerValues.BoundingBox);
        }

        public IEnumerable<ExtractFile> CreateReferencePointsFiles(ShapeContext context)
        {
            const string fileName = "Refpunt";
            const ShapeType shapeType = ShapeType.Point;
            int ReferencePointId(RoadReferencePointRecord record) => record.Id;
            Func<BinaryReader, ShapeContent> readShape = PointShapeContent.Read;

            // ToDo get projected record count so dbset is not executed
            var recordCount = context.RoadReferencePoints.Count();

            yield return CreateDbfFile<RoadReferencePointDbaseRecord>(
                fileName,
                new RoadReferencePointDbaseSchema(),
                context
                    .RoadReferencePoints
                    .OrderBy(ReferencePointId)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));

            // ToDo get projected header values
            var headerValues = CalculateShapeHeaderValues<RoadReferencePointRecord, PointShapeContent>(
                shapeType,
                context.RoadReferencePoints,
                record => record.ShapeRecordContent,
                readShape,
                content => content.Shape.EnvelopeInternal);

            yield return CreateShapeFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadReferencePoints
                    .OrderBy(ReferencePointId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                headerValues.ShapeFileLength,
                headerValues.BoundingBox);

            yield return CreateShapeIndexFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadReferencePoints
                    .OrderBy(ReferencePointId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                headerValues.ShapeIndexFileLegth,
                headerValues.BoundingBox);
        }

        public ExtractFile CreateOrganizationsFile(ShapeContext context)
        {
            // ToDo get projected record count so set is not executed
            var recordCount = context.Organizations.Count();

            return CreateDbfFile<OrganizationDbaseRecord>(
                "LstOrg",
                new OrganizationDbaseSchema(),
                context
                    .Organizations
                    .OrderBy(record => record.SortableCode)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount)
            );
        }

        public ExtractFile CreateRoadSegmentDynamicLaneAttributesFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.RoadLaneAttributes.Count();

            return CreateDbfFile<RoadSegmentDynamicLaneAttributeDbaseRecord>(
                "AttRijstroken",
                new RoadSegmentDynamicLaneAttributeDbaseSchema(),
                context
                    .RoadLaneAttributes
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount)
            );
        }

        public ExtractFile CreateRoadSegmentDynamicWidtAttributesFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.RoadWidthAttributes.Count();

            return CreateDbfFile<RoadSegmentDynamicWidthAttributeDbaseRecord>(
                "AttWegbreedte",
                new RoadSegmentDynamicWidthAttributeDbaseSchema(),
                context
                    .RoadWidthAttributes
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));
        }

        public ExtractFile CreateRoadSegmentDynamicHardeningAttributesFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.RoadHardeningAttributes.Count();

            return CreateDbfFile<RoadSegmentDynamicHardeningAttributeDbaseRecord>(
                "AttWegverharding",
                new RoadSegmentDynamicHardeningAttributeDbaseSchema(),
                context
                    .RoadHardeningAttributes
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));
        }

        public ExtractFile CreateRoadSegmentNationalRoadAttributesFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.NationalRoadAttributes.Count();

            return CreateDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(
                "AttNationweg",
                new RoadSegmentNationalRoadAttributeDbaseSchema(),
                context
                    .NationalRoadAttributes
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));
        }

        public ExtractFile CreateRoadSegmentEuropeanRoadAttributesFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.EuropeanRoadAttributes.Count();

            return CreateDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                "AttEuropweg",
                new RoadSegmentEuropeanRoadAttributeDbaseSchema(),
                context
                    .EuropeanRoadAttributes
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));
        }

        public ExtractFile CreateRoadSegmentNumberedRoadAttributesFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.NumberedRoadAttributes.Count();

            return CreateDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                "AttGenumweg",
                new RoadSegmentNumberedRoadAttributeDbaseSchema(),
                context
                    .NumberedRoadAttributes
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));
        }

        public ExtractFile CreateGradeSeperatedJunctionsFile(ShapeContext context)
        {
            // ToDo get projected record count so dbset is not executed
            var recordCount = context.GradeSeparatedJunctions.Count();

            return CreateDbfFile<GradeSeparatedJunctionDbaseRecord>(
                "RltOgkruising",
                new GradeSeparatedJunctionDbaseSchema(),
                context
                    .GradeSeparatedJunctions
                    .OrderBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                new DbaseRecordCount(recordCount));
        }

        public ExtractFile CreateRoadNodeTypesFile()
        {
            return CreateDbfFile(
                "WegknoopLktType",
                new RoadNodeTypeDbaseSchema(),
                TypeReferences.RoadNodeTypes,
                new DbaseRecordCount(TypeReferences.RoadNodeTypes.Length)
            );
        }

        public ExtractFile CreateRoadSegmentCategoriesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktWegcat",
                new RoadSegmentCategoryDbaseSchema(),
                TypeReferences.RoadSegmentCategories,
                new DbaseRecordCount(TypeReferences.RoadSegmentCategories.Length)
            );
        }

        public ExtractFile CreateRoadSegmentGeometryDrawMethodsFile()
        {
            return CreateDbfFile(
                "WegsegmentLktMethode",
                new RoadSegmentGeometryDrawMethodDbaseSchema(),
                TypeReferences.RoadSegmentGeometryDrawMethods,
                new DbaseRecordCount(TypeReferences.RoadSegmentGeometryDrawMethods.Length)
            );
        }

        public ExtractFile CreateRoadSegmentStatusesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktStatus",
                new RoadSegmentStatusDbaseSchema(),
                TypeReferences.RoadSegmentStatuses,
                new DbaseRecordCount(TypeReferences.RoadSegmentStatuses.Length)
            );
        }

        public ExtractFile CreateRoadSegmentAccessRestrictionsFile()
        {
            return CreateDbfFile(
                "WegsegmentLktTgbep",
                new RoadSegmentAccessRestrictionDbaseSchema(),
                TypeReferences.RoadSegmentAccessRestrictions,
                new DbaseRecordCount(TypeReferences.RoadSegmentAccessRestrictions.Length)
            );
        }

        public ExtractFile CreateReferencePointTypesFile()
        {
            return CreateDbfFile(
                "RefpuntLktType",
                new ReferencePointTypeDbaseSchema(),
                TypeReferences.ReferencePointTypes,
                new DbaseRecordCount(TypeReferences.ReferencePointTypes.Length)
            );
        }

        public ExtractFile CreateRoadSegmentMorphologiesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktMorf",
                new RoadSegmentMorphologyDbaseSchema(),
                TypeReferences.RoadSegmentMorphologies,
                new DbaseRecordCount(TypeReferences.RoadSegmentMorphologies.Length)
            );
        }

        public ExtractFile CreateGradeSeperatedJunctionTypesFile()
        {
            return CreateDbfFile(
                "OgkruisingLktType",
                new GradeSeparatedJunctionTypeDbaseSchema(),
                TypeReferences.GradeSeparatedJunctionTypes,
                new DbaseRecordCount(TypeReferences.GradeSeparatedJunctionTypes.Length)
            );
        }

        public ExtractFile CreateNumberedRoadSegmentDirectionsFile()
        {
            return CreateDbfFile(
                "GenumwegLktRichting",
                new NumberedRoadSegmentDirectionDbaseSchema(),
                TypeReferences.NumberedRoadSegmentDirections,
                new DbaseRecordCount(TypeReferences.NumberedRoadSegmentDirections.Length)
            );
        }

        public ExtractFile CreateHardeningTypesFile()
        {
            return CreateDbfFile(
                "WegverhardLktType",
                new HardeningTypeDbaseSchema(),
                TypeReferences.HardeningTypes,
                new DbaseRecordCount(TypeReferences.HardeningTypes.Length)
            );
        }

        public ExtractFile CreateLaneDirectionsFile()
        {
            return CreateDbfFile(
                "RijstrokenLktRichting",
                new LaneDirectionDbaseSchema(),
                TypeReferences.LaneDirections,
                new DbaseRecordCount(TypeReferences.LaneDirections.Length)
            );
        }

        private ExtractFile CreateDbfFile<TDbaseRecord>(
            string fileName,
            DbaseSchema schema,
            IEnumerable<byte[]> records,
            DbaseRecordCount recordCount
        ) where TDbaseRecord : DbaseRecord, new()
        {
            return new ExtractFile(
                new DbfFileName(fileName),
                (stream, token) =>
                {
                    var dbfFile = CreateDbfFileWriter<TDbaseRecord>(
                        schema,
                        recordCount,
                        stream
                    );

                    foreach (var record in records)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        dbfFile.WriteBytesAs<TDbaseRecord>(record);
                    }
                    dbfFile.WriteEndOfFile();
                }
            );
        }

        private ExtractFile CreateDbfFile<TDbaseRecord>(
            string fileName,
            DbaseSchema schema,
            IEnumerable<TDbaseRecord> records,
            DbaseRecordCount recordCount
        ) where TDbaseRecord : DbaseRecord
        {
            return new ExtractFile(
                new DbfFileName(fileName),
                (stream, token) =>
                {
                    var dbfFileWriter = CreateDbfFileWriter<TDbaseRecord>(
                        schema,
                        recordCount,
                        stream
                    );

                    foreach (var record in records)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        dbfFileWriter.Write(record);
                    }
                    dbfFileWriter.WriteEndOfFile();
                }
            );
        }

        private static DbfFileWriter<TDbaseRecord> CreateDbfFileWriter<TDbaseRecord>(DbaseSchema schema, DbaseRecordCount recordCount, Stream writeStream)
            where TDbaseRecord : DbaseRecord
        {
            return new DbfFileWriter<TDbaseRecord>(
                new DbaseFileHeader(
                    DateTime.Now,
                    DbaseCodePage.Western_European_ANSI,
                    recordCount,
                    schema
                ),
                writeStream
            );
        }

        private static ExtractFile CreateShapeFile<TShape>(
            string fileName,
            ShapeType shapeType,
            IEnumerable<byte[]> shapes,
            Func<BinaryReader, ShapeContent> readShape,
            WordLength shapeFileLength,
            BoundingBox3D boundingBox
        ) where TShape : ShapeContent
        {
            return new ExtractFile(
                new ShpFileName(fileName),
                (stream, token) =>
                {
                    var shpFile = new ShpFileWriter(
                        new ShapeFileHeader(
                            shapeFileLength,
                            shapeType,
                            boundingBox
                        ),
                        stream
                    );

                    var number = RecordNumber.Initial;
                    foreach (var shape in shapes)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        using (var shapeStream = new MemoryStream(shape))
                        using (var reader = new BinaryReader(shapeStream))
                        {
                            var content = readShape(reader);
                            if (content is TShape || content is NullShapeContent)
                            {
                                var shapeRecord = content.RecordAs(number);
                                shpFile.Write(shapeRecord);

                                number = number.Next();
                            }
                        }
                    }
                }
            );
        }

        private static ExtractFile CreateShapeIndexFile<TShape>(
            string fileName,
            ShapeType shapeType,
            IEnumerable<byte[]> shapes,
            Func<BinaryReader, ShapeContent> readShape,
            WordLength indexFileLegth,
            BoundingBox3D boundingBox
        ) where TShape : ShapeContent
        {
            return new ExtractFile(
                new ShxFileName(fileName),
                (stream, token) =>
                {
                    var shxFileWriter = new ShxFileWriter(
                        new ShapeFileHeader(
                            indexFileLegth,
                            shapeType,
                            boundingBox
                        ),
                        stream
                    );

                    var number = RecordNumber.Initial;
                    var offset = ShapeRecord.InitialOffset;
                    foreach (var shapeBytes in shapes)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        using (var shapeStream = new MemoryStream(shapeBytes))
                        using (var reader = new BinaryReader(shapeStream))
                        {
                            var content = readShape(reader);
                            if (content is TShape || content is NullShapeContent)
                            {
                                var shapeRecord = content.RecordAs(number);

                                var indexRecord = shapeRecord.IndexAt(offset);
                                shxFileWriter.Write(indexRecord);

                                number = number.Next();
                                offset = offset.Plus(shapeRecord.Length);
                            }
                        }
                    }
                }
            );
        }

        private static ShapeHeaderValues CalculateShapeHeaderValues<TRecord, TShape>(
            ShapeType shapeType,
            IEnumerable<TRecord> records,
            Func<TRecord, byte[]> getShapeBytes,
            Func<BinaryReader, ShapeContent> readShape,
            Func<TShape, Envelope> getEnvelope
        ) where TShape : ShapeContent
        {
            var envelope = new Envelope();
            var shapeFileLength = new WordLength(ShapeRecord.InitialOffset);
            var indexFileLegth = new WordLength(ShapeRecord.InitialOffset);

            var number = RecordNumber.Initial;
            foreach (var record in records)
            {
                using (var stream = new MemoryStream(getShapeBytes(record)))
                using (var reader = new BinaryReader(stream))
                {
                    var content = readShape(reader);
                    if (typeof(TShape) != typeof(NullShapeContent) && content is TShape shapeContent)
                        envelope.ExpandToInclude(getEnvelope(shapeContent));

                    if (content is TShape || content is NullShapeContent)
                    {
                        var shapeRecord = content.RecordAs(number);
                        shapeFileLength = shapeFileLength.Plus(shapeRecord.Length);
                        indexFileLegth = indexFileLegth.Plus(new WordLength(4));

                        number = number.Next();
                    }
                }
            }

            var mMin = shapeType == ShapeType.PolyLineM ? double.NegativeInfinity : 0;
            var mMax = shapeType == ShapeType.PolyLineM ? double.PositiveInfinity : 0;
            var boundingBox = new BoundingBox3D(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY, 0, 0, mMin, mMax);

            // ToDo get projected shp file length
            // ToDo get projected shx file length
            // ToDo get projected boundingbox
            return new ShapeHeaderValues(
                shapeFileLength,
                indexFileLegth,
                boundingBox
            );
        }

        private class ShapeHeaderValues
        {
            internal WordLength ShapeFileLength { get; }
            internal WordLength ShapeIndexFileLegth { get; }
            internal BoundingBox3D BoundingBox { get; }

            public ShapeHeaderValues(
                WordLength shapeFileLength,
                WordLength shapeIndexFileLegth,
                BoundingBox3D boundingBox)
            {
                ShapeFileLength = shapeFileLength;
                ShapeIndexFileLegth = shapeIndexFileLegth;
                BoundingBox = boundingBox;
            }
        }
    }
}
