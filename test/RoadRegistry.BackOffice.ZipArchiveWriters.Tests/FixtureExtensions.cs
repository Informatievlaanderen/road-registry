using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;
using System.IO.Compression;
using System.Text;

namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests;

using Extracts.Dbase;

internal static class FixtureExtensions
{
    public static MemoryStream CreateDbfFileWithOneRecord<T>(this IFixture fixture, DbaseSchema schema, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        return CreateDbfFileWithOneRecord(fixture, schema, fixture.Create<T>(), updateRecord);
    }

    public static MemoryStream CreateDbfFileWithOneRecord<T>(this IFixture fixture, DbaseSchema schema, T record, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        return CreateDbfFile(fixture, schema, new[] { record }, updateRecord);
    }

    public static MemoryStream CreateEmptyDbfFile<T>(this IFixture fixture, DbaseSchema schema)
        where T : DbaseRecord
    {
        return CreateDbfFile(fixture, schema, Array.Empty<T>());
    }

    public static MemoryStream CreateDbfFile<T>(this IFixture fixture, DbaseSchema schema, ICollection<T> records, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        var dbaseChangeStream = new MemoryStream();

        using (var writer = new DbaseBinaryWriter(
           new DbaseFileHeader(
               fixture.Create<DateTime>(),
               DbaseCodePage.Western_European_ANSI,
               new DbaseRecordCount(records.Count),
               schema),
           new BinaryWriter(
               dbaseChangeStream,
               Encoding.UTF8,
               true)))
        {
            if (records.Any())
            {
                foreach (var record in records)
                {
                    updateRecord?.Invoke(record);
                    writer.Write(record);
                }
            }
            else
            {
                writer.Write(Array.Empty<T>());
            }
        }

        return dbaseChangeStream;
    }

    public static MemoryStream CreateProjectionFormatFileWithOneRecord(this IFixture fixture)
    {
        var projectionFormatStream = new MemoryStream();
        using (var writer = new StreamWriter(
                   projectionFormatStream,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            writer.Write(ProjectionFormat.BelgeLambert1972.Content);
        }

        return projectionFormatStream;
    }

    public static MemoryStream CreateEmptyProjectionFormatFile(this IFixture fixture)
    {
        var projectionFormatStream = new MemoryStream();
        using (var writer = new StreamWriter(
                   projectionFormatStream,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            writer.Write(string.Empty);
        }

        return projectionFormatStream;
    }

    public static MemoryStream CreateRoadSegmentShapeFileWithOneRecord(this IFixture fixture, PolyLineMShapeContent polyLineMShapeContent = null)
    {
        if (polyLineMShapeContent is null)
        {
            polyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
        }

        return CreateRoadSegmentShapeFile(fixture, new[] { polyLineMShapeContent });
    }

    public static MemoryStream CreateEmptyRoadSegmentShapeFile(this IFixture fixture)
    {
        return CreateRoadSegmentShapeFile(fixture, Array.Empty<PolyLineMShapeContent>());
    }

    public static MemoryStream CreateRoadSegmentShapeFile(this IFixture fixture, ICollection<PolyLineMShapeContent> shapes)
    {
        return CreateShapeFile(ShapeType.PolyLineM, shapes, shape => BoundingBox3D.FromGeometry(shape.Shape));
    }

    public static MemoryStream CreateRoadNodeShapeFileWithOneRecord(this IFixture fixture)
    {
        return CreateRoadNodeShapeFile(fixture, new[] { fixture.Create<PointShapeContent>() });
    }

    public static MemoryStream CreateEmptyRoadNodeShapeFile(this IFixture fixture)
    {
        return CreateRoadNodeShapeFile(fixture, Array.Empty<PointShapeContent>());
    }

    public static MemoryStream CreateRoadNodeShapeFile(this IFixture fixture, ICollection<PointShapeContent> shapes)
    {
        return CreateShapeFile(ShapeType.Point, shapes, shape => BoundingBox3D.FromGeometry(shape.Shape));
    }

    private static MemoryStream CreateShapeFile<TShapeContent>(ShapeType shapeType, ICollection<TShapeContent> shapes, Func<TShapeContent, BoundingBox3D> getBoundingBox3D)
        where TShapeContent : ShapeContent
    {
        ArgumentNullException.ThrowIfNull(shapes);

        var roadNodeShapeChangeStream = new MemoryStream();

        var shapeRecords = new List<ShapeRecord>();
        var fileWordLength = ShapeFileHeader.Length;
        var boundingBox3D = BoundingBox3D.Empty;
        var recordNumber = RecordNumber.Initial;
        foreach (var shape in shapes)
        {
            boundingBox3D = boundingBox3D.ExpandWith(getBoundingBox3D(shape));

            var shapeRecord = shape.RecordAs(recordNumber);
            fileWordLength = fileWordLength.Plus(shapeRecord.Length);
            shapeRecords.Add(shapeRecord);

            recordNumber = recordNumber.Next();
        }

        using (var writer = new ShapeBinaryWriter(
           new ShapeFileHeader(
               fileWordLength,
               shapeType,
               boundingBox3D),
           new BinaryWriter(
               roadNodeShapeChangeStream,
               Encoding.UTF8,
               true)))
        {
            if (shapeRecords.Any())
            {
                foreach (var shapeRecord in shapeRecords)
                {
                    writer.Write(shapeRecord);
                }
            }
            else
            {
                writer.Write(Array.Empty<ShapeRecord>());
            }
        }

        return roadNodeShapeChangeStream;
    }

    public static ZipArchive CreateUploadZipArchive(this Fixture fixture, ExtractsZipArchiveTestData testData,
        MemoryStream roadSegmentShapeChangeStream = null,
        MemoryStream roadSegmentProjectionFormatStream = null,
        MemoryStream roadSegmentDbaseChangeStream = null,
        MemoryStream roadNodeShapeChangeStream = null,
        MemoryStream roadNodeProjectionFormatStream = null,
        MemoryStream roadNodeDbaseChangeStream = null,
        MemoryStream europeanRoadChangeStream = null,
        MemoryStream numberedRoadChangeStream = null,
        MemoryStream nationalRoadChangeStream = null,
        MemoryStream laneChangeStream = null,
        MemoryStream widthChangeStream = null,
        MemoryStream surfaceChangeStream = null,
        MemoryStream gradeSeparatedJunctionChangeStream = null,
        MemoryStream roadSegmentShapeExtractStream = null,
        MemoryStream roadSegmentDbaseExtractStream = null,
        MemoryStream roadNodeShapeExtractStream = null,
        MemoryStream roadNodeDbaseExtractStream = null,
        MemoryStream europeanRoadExtractStream = null,
        MemoryStream numberedRoadExtractStream = null,
        MemoryStream nationalRoadExtractStream = null,
        MemoryStream laneExtractStream = null,
        MemoryStream widthExtractStream = null,
        MemoryStream surfaceExtractStream = null,
        MemoryStream gradeSeparatedJunctionExtractStream = null,
        MemoryStream transactionZoneStream = null
    )
    {
        if (transactionZoneStream is null)
        {
            transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);
        }

        var files = new Dictionary<string, Stream>
            {
                { "WEGSEGMENT.SHP", roadSegmentShapeChangeStream },
                { "EWEGSEGMENT.SHP", roadSegmentShapeExtractStream },
                { "WEGSEGMENT.DBF", roadSegmentDbaseChangeStream },
                { "EWEGSEGMENT.DBF", roadSegmentDbaseExtractStream },
                { "WEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
                { "WEGKNOOP.SHP", roadNodeShapeChangeStream },
                { "EWEGKNOOP.SHP", roadNodeShapeExtractStream },
                { "WEGKNOOP.DBF", roadNodeDbaseChangeStream },
                { "EWEGKNOOP.DBF", roadNodeDbaseExtractStream },
                { "WEGKNOOP.PRJ", roadNodeProjectionFormatStream },
                { "ATTEUROPWEG.DBF", europeanRoadChangeStream },
                { "EATTEUROPWEG.DBF", europeanRoadExtractStream },
                { "ATTGENUMWEG.DBF", numberedRoadChangeStream },
                { "EATTGENUMWEG.DBF", numberedRoadExtractStream },
                { "ATTNATIONWEG.DBF", nationalRoadChangeStream },
                { "EATTNATIONWEG.DBF", nationalRoadExtractStream },
                { "ATTRIJSTROKEN.DBF", laneChangeStream },
                { "EATTRIJSTROKEN.DBF", laneExtractStream },
                { "ATTWEGBREEDTE.DBF", widthChangeStream },
                { "EATTWEGBREEDTE.DBF", widthExtractStream },
                { "ATTWEGVERHARDING.DBF", surfaceChangeStream },
                { "EATTWEGVERHARDING.DBF", surfaceExtractStream },
                { "RLTOGKRUISING.DBF", gradeSeparatedJunctionChangeStream },
                { "ERLTOGKRUISING.DBF", gradeSeparatedJunctionExtractStream },
                { "TRANSACTIEZONES.DBF", transactionZoneStream }
            };

        var random = new Random(fixture.Create<int>());
        var writeOrder = files.Keys.OrderBy(_ => random.Next()).ToArray();

        var archiveStream = new MemoryStream();
        using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            foreach (var file in writeOrder)
            {
                var stream = files[file];
                if (stream is not null)
                {
                    stream.Position = 0;
                    using (var entryStream = createArchive.CreateEntry(file).Open())
                    {
                        stream.CopyTo(entryStream);
                    }
                }
                else
                {
                    var extractFileEntry = testData.ZipArchiveWithEmptyFiles.Entries.Single(x => x.Name == file);
                    using (var extractFileEntryStream = extractFileEntry.Open())
                    using (var entryStream = createArchive.CreateEntry(file).Open())
                    {
                        extractFileEntryStream.CopyTo(entryStream);
                    }
                }
            }
        }
        archiveStream.Position = 0;

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
    }
}
