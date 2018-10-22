namespace RoadRegistry.Api.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using Aiv.Vbr.Shaperon;
    using ExtractFiles;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public class RoadRegistryExtractsBuilder
    {
        public IEnumerable<ExtractFile> CreateRoadSegmentsFiles(ShapeContext context)
        {
            const string fileName = "Wegsegment";
            const ShapeType shapeType = ShapeType.PolyLineM;
            Func<BinaryReader, ShapeContent> readShape = PolyLineMShapeContent.Read;
            Expression<Func<RoadSegmentRecord, int>> segmentId = record => record.Id;

            yield return CreateDbfFile<RoadSegmentDbaseRecord>(
                fileName,
                new RoadSegmentDbaseSchema(),
                context
                    .RoadSegments
                    .OrderQueryBy(segmentId)
                    .Select(record => record.DbaseRecord),
                context.RoadSegments.Count
            );

            var boundingBox =
                context
                    .RoadSegments
                    .GroupBy(record => 1)
                    .Select(all_records => new BoundingBox2D
                {
                    MinimumX = all_records.Min(record => record.Envelope.MinimumX),
                    MaximumX = all_records.Max(record => record.Envelope.MaximumX),
                    MinimumY = all_records.Min(record => record.Envelope.MinimumY),
                    MaximumY = all_records.Max(record => record.Envelope.MaximumY)
                })
                .SingleOrDefault()?.ToBoundingBox3D() ??
                new BoundingBox3D(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, double.NegativeInfinity, double.PositiveInfinity);

            yield return CreateShapeFile<PolyLineMShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadSegments
                    .OrderQueryBy(segmentId)
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
                    .OrderQueryBy(segmentId)
                    .Select(record => record.ShapeRecordContentLength),
                context.RoadSegments.Count,
                boundingBox
            );
        }

        public IEnumerable<ExtractFile> CreateRoadNodesFiles(ShapeContext context)
        {
            const string fileName = "Wegknoop";
            const ShapeType shapeType = ShapeType.Point;
            Func<BinaryReader, ShapeContent> readShape = PointShapeContent.Read;
            Expression<Func<RoadNodeRecord,int>> nodeId = record => record.Id;

            yield return CreateDbfFile<RoadNodeDbaseRecord>(
                fileName,
                new RoadNodeDbaseSchema(),
                context
                    .RoadNodes
                    .OrderQueryBy(nodeId)
                    .Select(record => record.DbaseRecord),
                context.RoadNodes.Count
            );

            var boundingBox =
                context
                    .RoadNodes
                    .GroupBy(record => 1)
                    .Select(all_records => new BoundingBox2D
                {
                    MinimumX = all_records.Min(record => record.Envelope.MinimumX),
                    MaximumX = all_records.Max(record => record.Envelope.MaximumX),
                    MinimumY = all_records.Min(record => record.Envelope.MinimumY),
                    MaximumY = all_records.Max(record => record.Envelope.MaximumY)
                })
                .SingleOrDefault()?.ToBoundingBox3D() ??
                new BoundingBox3D(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

            yield return CreateShapeFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadNodes
                    .OrderQueryBy(nodeId)
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
                    .OrderQueryBy(nodeId)
                    .Select(record => record.ShapeRecordContentLength),
                context.RoadNodes.Count,
                boundingBox
            );
        }

        public IEnumerable<ExtractFile> CreateReferencePointsFiles(ShapeContext context)
        {
            const string fileName = "Refpunt";
            const ShapeType shapeType = ShapeType.Point;
            Expression<Func<RoadReferencePointRecord, int>> referencePointId = record => record.Id;
            Func<BinaryReader, ShapeContent> readShape = PointShapeContent.Read;

            yield return CreateDbfFile<RoadReferencePointDbaseRecord>(
                fileName,
                new RoadReferencePointDbaseSchema(),
                context
                    .RoadReferencePoints
                    .OrderQueryBy(referencePointId)
                    .Select(record => record.DbaseRecord),
                context.RoadReferencePoints.Count
            );

            var boundingBox =
                context
                    .RoadReferencePoints
                    .GroupBy(record => 1)
                    .Select(all_records => new BoundingBox2D
                {
                    MinimumX = all_records.Min(record => record.Envelope.MinimumX),
                    MaximumX = all_records.Max(record => record.Envelope.MaximumX),
                    MinimumY = all_records.Min(record => record.Envelope.MinimumY),
                    MaximumY = all_records.Max(record => record.Envelope.MaximumY)
                })
                .SingleOrDefault()?.ToBoundingBox3D() ??
                new BoundingBox3D(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

            yield return CreateShapeFile<PointShapeContent>(
                fileName,
                shapeType,
                context
                    .RoadReferencePoints
                    .OrderQueryBy(referencePointId)
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
                    .OrderQueryBy(referencePointId)
                    .Select(record => record.ShapeRecordContentLength),
                context.RoadReferencePoints.Count,
                boundingBox
            );
        }

        public ExtractFile CreateOrganizationsFile(DbSet<OrganizationRecord> organizations)
        {
            return CreateDbfFile<OrganizationDbaseRecord>(
                "LstOrg",
                new OrganizationDbaseSchema(),
                organizations
                    .OrderQueryBy(record => record.SortableCode)
                    .Select(record => record.DbaseRecord),
                organizations.Count
            );
        }

        public ExtractFile CreateRoadSegmentDynamicLaneAttributesFile(DbSet<RoadSegmentDynamicLaneAttributeRecord> roadLaneAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicLaneAttributeDbaseRecord>(
                "AttRijstroken",
                new RoadSegmentDynamicLaneAttributeDbaseSchema(),
                roadLaneAttributes
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                roadLaneAttributes.Count
            );
        }

        public ExtractFile CreateRoadSegmentDynamicWidtAttributesFile(DbSet<RoadSegmentDynamicWidthAttributeRecord> roadWidthAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicWidthAttributeDbaseRecord>(
                "AttWegbreedte",
                new RoadSegmentDynamicWidthAttributeDbaseSchema(),
                roadWidthAttributes
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                roadWidthAttributes.Count);
        }

        public ExtractFile CreateRoadSegmentDynamicHardeningAttributesFile(DbSet<RoadSegmentDynamicHardeningAttributeRecord> roadHardeningAttributes)
        {
            return CreateDbfFile<RoadSegmentDynamicHardeningAttributeDbaseRecord>(
                "AttWegverharding",
                new RoadSegmentDynamicHardeningAttributeDbaseSchema(),
                roadHardeningAttributes
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                roadHardeningAttributes.Count
            );
        }

        public ExtractFile CreateRoadSegmentNationalRoadAttributesFile(DbSet<RoadSegmentNationalRoadAttributeRecord> nationalRoadAttributes)
        {
            return CreateDbfFile<RoadSegmentNationalRoadAttributeDbaseRecord>(
                "AttNationweg",
                new RoadSegmentNationalRoadAttributeDbaseSchema(),
                nationalRoadAttributes
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                nationalRoadAttributes.Count
            );
        }

        public ExtractFile CreateRoadSegmentEuropeanRoadAttributesFile(DbSet<RoadSegmentEuropeanRoadAttributeRecord> europeanRoadAttributes)
        {
            return CreateDbfFile<RoadSegmentEuropeanRoadAttributeDbaseRecord>(
                "AttEuropweg",
                new RoadSegmentEuropeanRoadAttributeDbaseSchema(),
                europeanRoadAttributes
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                europeanRoadAttributes.Count
            );
        }

        public ExtractFile CreateRoadSegmentNumberedRoadAttributesFile(DbSet<RoadSegmentNumberedRoadAttributeRecord> numberedRoadAttributes)
        {
            return CreateDbfFile<RoadSegmentNumberedRoadAttributeDbaseRecord>(
                "AttGenumweg",
                new RoadSegmentNumberedRoadAttributeDbaseSchema(),
                numberedRoadAttributes
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                numberedRoadAttributes.Count
            );
        }

        public ExtractFile CreateGradeSeperatedJunctionsFile(DbSet<GradeSeparatedJunctionRecord> gradeSeparatedJunctions)
        {
            return CreateDbfFile<GradeSeparatedJunctionDbaseRecord>(
                "RltOgkruising",
                new GradeSeparatedJunctionDbaseSchema(),
                gradeSeparatedJunctions
                    .OrderQueryBy(record => record.Id)
                    .Select(record => record.DbaseRecord),
                gradeSeparatedJunctions.Count
            );
        }

        public ExtractFile CreateRoadNodeTypesFile()
        {
            return CreateDbfFile(
                "WegknoopLktType",
                new RoadNodeTypeDbaseSchema(),
                ReferenceData.RoadNodeTypes
            );
        }

        public ExtractFile CreateRoadSegmentCategoriesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktWegcat",
                new RoadSegmentCategoryDbaseSchema(),
                ReferenceData.RoadSegmentCategories
            );
        }

        public ExtractFile CreateRoadSegmentGeometryDrawMethodsFile()
        {
            return CreateDbfFile(
                "WegsegmentLktMethode",
                new RoadSegmentGeometryDrawMethodDbaseSchema(),
                ReferenceData.RoadSegmentGeometryDrawMethods
            );
        }

        public ExtractFile CreateRoadSegmentStatusesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktStatus",
                new RoadSegmentStatusDbaseSchema(),
                ReferenceData.RoadSegmentStatuses
            );
        }

        public ExtractFile CreateRoadSegmentAccessRestrictionsFile()
        {
            return CreateDbfFile(
                "WegsegmentLktTgbep",
                new RoadSegmentAccessRestrictionDbaseSchema(),
                ReferenceData.RoadSegmentAccessRestrictions
            );
        }

        public ExtractFile CreateReferencePointTypesFile()
        {
            return CreateDbfFile(
                "RefpuntLktType",
                new ReferencePointTypeDbaseSchema(),
                ReferenceData.ReferencePointTypes
            );
        }

        public ExtractFile CreateRoadSegmentMorphologiesFile()
        {
            return CreateDbfFile(
                "WegsegmentLktMorf",
                new RoadSegmentMorphologyDbaseSchema(),
                ReferenceData.RoadSegmentMorphologies
            );
        }

        public ExtractFile CreateGradeSeperatedJunctionTypesFile()
        {
            return CreateDbfFile(
                "OgkruisingLktType",
                new GradeSeparatedJunctionTypeDbaseSchema(),
                ReferenceData.GradeSeparatedJunctionTypes
            );
        }

        public ExtractFile CreateNumberedRoadSegmentDirectionsFile()
        {
            return CreateDbfFile(
                "GenumwegLktRichting",
                new NumberedRoadSegmentDirectionDbaseSchema(),
                ReferenceData.NumberedRoadSegmentDirections
            );
        }

        public ExtractFile CreateHardeningTypesFile()
        {
            return CreateDbfFile(
                "WegverhardLktType",
                new HardeningTypeDbaseSchema(),
                ReferenceData.HardeningTypes
            );
        }

        public ExtractFile CreateLaneDirectionsFile()
        {
            return CreateDbfFile(
                "RijstrokenLktRichting",
                new LaneDirectionDbaseSchema(),
                ReferenceData.LaneDirections
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
            IEnumerable<byte[]> records,
            Func<int> getRecordCount
        ) where TDbaseRecord : DbaseRecord, new()
        {
            return new ExtractFile(
                new DbfFileName(fileName),
                (stream, token) =>
                {
                    var dbfFile = CreateDbfFileWriter<TDbaseRecord>(
                        schema,
                        new DbaseRecordCount(getRecordCount()),
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
            IReadOnlyCollection<TDbaseRecord> records
        ) where TDbaseRecord : DbaseRecord
        {
            return new ExtractFile(
                new DbfFileName(fileName),
                (stream, token) =>
                {
                    var dbfFileWriter = CreateDbfFileWriter<TDbaseRecord>(
                        schema,
                        new DbaseRecordCount(records.Count),
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

        private static DbfFileWriter<TDbaseRecord> CreateDbfFileWriter<TDbaseRecord>(
            DbaseSchema schema,
            DbaseRecordCount recordCount,
            Stream writeStream
        ) where TDbaseRecord : DbaseRecord
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
                    var totalShapeRecordsLength = shapeLengths.Sum(shapeLength => shapeLength + ShapeRecord.HeaderLength);
                    var shpFile = new ShpFileWriter(
                        new ShapeFileHeader(
                            new WordLength(ShapeRecord.InitialOffset).Plus(new WordLength(totalShapeRecordsLength)),
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
            Func<int> getRecordCount,
            BoundingBox3D boundingBox
        )
        {
            return new ExtractFile(
                new ShxFileName(fileName),
                (stream, token) =>
                {
                    var shxFileWriter = new ShxFileWriter(
                        new ShapeFileHeader(
                            new WordLength(ShapeRecord.InitialOffset).Plus(new WordLength(getRecordCount() * 4)),
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
