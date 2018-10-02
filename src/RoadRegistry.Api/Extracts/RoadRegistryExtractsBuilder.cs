namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using ExtractFiles;
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
            var recordCount = new DbaseRecordCount(context.RoadSegments.Count());

            yield return CreateDbfFile<RoadSegmentDbaseRecord>(
                fileName,
                new RoadSegmentDbaseSchema(),
                context
                    .RoadSegments
                    .OrderBy(SegmentId)
                    .Select(record => record.DbaseRecord),
                recordCount
            );

            // ToDo get values from a single DB call, project boundries instead of making 4 calls
            Console.WriteLine($"-- Calculating {fileName} boundry values");
            var stopwatch = Stopwatch.StartNew();
            var boundingBox = new BoundingBox3D(
                context.RoadSegments.Min(record => record.Envelope.MinimumX),
                context.RoadSegments.Min(record => record.Envelope.MinimumY),
                context.RoadSegments.Max(record => record.Envelope.MaximumX),
                context.RoadSegments.Max(record => record.Envelope.MaximumY),
                0,
                0,
                double.NegativeInfinity,
                double.PositiveInfinity
            );
            Console.WriteLine($"-- It took {stopwatch.ElapsedMilliseconds}ms to calculate {fileName} boundry values");

            yield return CreateShapeFile<PolyLineMShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadSegments
                    .OrderBy(SegmentId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                context
                    .RoadSegments
                    .Select(record => record.ShapeRecordContentLength),
                boundingBox
            );

            yield return CreateShapeIndexFile(
                fileName,
                shapeType,
                context
                    .RoadSegments
                    .OrderBy(SegmentId)
                    .Select(record => record.ShapeRecordContentLength),
                recordCount,
                boundingBox
            );
        }

        public IEnumerable<ExtractFile> CreateRoadNodesFiles(ShapeContext context)
        {
            const string fileName = "Wegknoop";
            const ShapeType shapeType = ShapeType.Point;
            Func<BinaryReader, ShapeContent> readShape = PointShapeContent.Read;
            int NodeId(RoadNodeRecord record) => record.Id;

            // ToDo get projected record count so dbset is not executed
            var recordCount = new DbaseRecordCount(context.RoadNodes.Count());

            yield return CreateDbfFile<RoadNodeDbaseRecord>(
                fileName,
                new RoadNodeDbaseSchema(),
                context
                    .RoadNodes
                    .OrderBy(NodeId)
                    .Select(record => record.DbaseRecord),
                recordCount
            );

            // ToDo get values from a single DB call, project boundries instead of making 4 calls
            Console.WriteLine($"-- Calculating {fileName} boundry values");
            var stopwatch = Stopwatch.StartNew();
            var boundingBox = new BoundingBox3D(
                context.RoadNodes.Min(record => record.Envelope.MinimumX),
                context.RoadNodes.Min(record => record.Envelope.MinimumY),
                context.RoadNodes.Max(record => record.Envelope.MaximumX),
                context.RoadNodes.Max(record => record.Envelope.MaximumY),
                0,
                0,
                0,
                0
            );
            Console.WriteLine($"-- It took {stopwatch.ElapsedMilliseconds}ms to calculate {fileName} boundry values");

            yield return CreateShapeFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadNodes
                    .OrderBy(NodeId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                context
                    .RoadNodes
                    .Select(record => record.ShapeRecordContentLength),
                boundingBox
            );

            yield return CreateShapeIndexFile(
                fileName,
                shapeType,
                context
                    .RoadNodes
                    .OrderBy(NodeId)
                    .Select(record => record.ShapeRecordContentLength),
                recordCount,
                boundingBox
            );
        }

        public IEnumerable<ExtractFile> CreateReferencePointsFiles(ShapeContext context)
        {
            const string fileName = "Refpunt";
            const ShapeType shapeType = ShapeType.Point;
            int ReferencePointId(RoadReferencePointRecord record) => record.Id;
            Func<BinaryReader, ShapeContent> readShape = PointShapeContent.Read;

            // ToDo get projected record count so dbset is not executed
            var recordCount = new DbaseRecordCount(context.RoadReferencePoints.Count());

            yield return CreateDbfFile<RoadReferencePointDbaseRecord>(
                fileName,
                new RoadReferencePointDbaseSchema(),
                context
                    .RoadReferencePoints
                    .OrderBy(ReferencePointId)
                    .Select(record => record.DbaseRecord),
                recordCount
            );

            // ToDo get values from a single DB call, project boundries instead of making 4 calls
            Console.WriteLine($"-- Calculating {fileName} boundry values");
            var stopwatch = Stopwatch.StartNew();
            var boundingBox = new BoundingBox3D(
                context.RoadReferencePoints.Min(record => record.Envelope.MinimumX),
                context.RoadReferencePoints.Min(record => record.Envelope.MinimumY),
                context.RoadReferencePoints.Max(record => record.Envelope.MaximumX),
                context.RoadReferencePoints.Max(record => record.Envelope.MaximumY),
                0,
                0,
                0,
                0
            );
            Console.WriteLine($"-- It took {stopwatch.ElapsedMilliseconds}ms to calculate {fileName} boundry values");

            yield return CreateShapeFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadReferencePoints
                    .OrderBy(ReferencePointId)
                    .Select(record => record.ShapeRecordContent),
                readShape,
                context
                    .RoadReferencePoints
                    .Select(record => record.ShapeRecordContentLength),
                boundingBox
            );

            yield return CreateShapeIndexFile(
                fileName,
                shapeType,
                context
                    .RoadReferencePoints
                    .OrderBy(ReferencePointId)
                    .Select(record => record.ShapeRecordContentLength),
                recordCount,
                boundingBox
            );
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
            IEnumerable<int> shapeLengths,
            BoundingBox3D boundingBox
        ) where TShape : ShapeContent
        {
            return new ExtractFile(
                new ShpFileName(fileName),
                (stream, token) =>
                {
                    var shpFile = new ShpFileWriter(
                        new ShapeFileHeader(
                            new WordLength(ShapeRecord.InitialOffset).Plus(new WordLength(shapeLengths.Sum())),
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

        private static ExtractFile CreateShapeIndexFile(
            string fileName,
            ShapeType shapeType,
            IEnumerable<int> shapesLengths,
            DbaseRecordCount recordCount,
            BoundingBox3D boundingBox
        )
        {
            return new ExtractFile(
                new ShxFileName(fileName),
                (stream, token) =>
                {
                    var shxFileWriter = new ShxFileWriter(
                        new ShapeFileHeader(
                            new WordLength(ShapeRecord.InitialOffset).Plus(new WordLength(recordCount * 4)),
                            shapeType,
                            boundingBox
                        ),
                        stream
                    );

                    var offset = ShapeRecord.InitialOffset;
                    foreach (var shapeLength in shapesLengths)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        var indexRecord = new ShapeIndexRecord(offset, new WordLength(shapeLength));
                        shxFileWriter.Write(indexRecord);

                        offset = offset.Plus(indexRecord.ContentLength.Plus(ShapeRecord.HeaderLength));
                    }
                }
            );
        }
    }
}
