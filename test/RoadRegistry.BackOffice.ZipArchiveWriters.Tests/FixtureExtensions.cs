using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using System.Text;

namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests;

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
        var dbaseChangeStream = new MemoryStream();

        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(1),
                       schema),
                   new BinaryWriter(
                       dbaseChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            updateRecord?.Invoke(record);
            writer.Write(record);
        }

        return dbaseChangeStream;
    }

    public static MemoryStream CreateEmptyDbfFile<T>(this IFixture fixture, DbaseSchema schema)
        where T : DbaseRecord
    {
        var dbaseChangeStream = new MemoryStream();

        using (var writer = new DbaseBinaryWriter(
                   new DbaseFileHeader(
                       fixture.Create<DateTime>(),
                       DbaseCodePage.Western_European_ANSI,
                       new DbaseRecordCount(0),
                       schema),
                   new BinaryWriter(
                       dbaseChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<T>());
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
            writer.Write(ProjectionFormat.BelgeLambert1972);
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

    public static MemoryStream CreateRoadSegmentShapeFileWithOneRecord(this IFixture fixture)
    {
        var roadSegmentShapeChangeStream = new MemoryStream();
        var polyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
        var roadSegmentShapeChangeRecord =
            polyLineMShapeContent.RecordAs(fixture.Create<RecordNumber>());
        using (var writer = new ShapeBinaryWriter(
                   new ShapeFileHeader(
                       roadSegmentShapeChangeRecord.Length.Plus(ShapeFileHeader.Length),
                       ShapeType.PolyLineM,
                       BoundingBox3D.FromGeometry(polyLineMShapeContent.Shape)),
                   new BinaryWriter(
                       roadSegmentShapeChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(roadSegmentShapeChangeRecord);
        }

        return roadSegmentShapeChangeStream;
    }

    public static MemoryStream CreateEmptyRoadSegmentShapeFile(this IFixture fixture)
    {
        var roadSegmentShapeChangeStream = new MemoryStream();
        using (var writer = new ShapeBinaryWriter(
                   new ShapeFileHeader(
                       ShapeFileHeader.Length,
                       ShapeType.PolyLineM,
                       BoundingBox3D.Empty),
                   new BinaryWriter(
                       roadSegmentShapeChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<ShapeRecord>());
        }

        return roadSegmentShapeChangeStream;
    }

    public static MemoryStream CreateRoadNodeShapeFileWithOneRecord(this IFixture fixture)
    {
        var roadNodeShapeChangeStream = new MemoryStream();
        var pointShapeContent = fixture.Create<PointShapeContent>();
        var roadNodeShapeChangeRecord =
            pointShapeContent.RecordAs(fixture.Create<RecordNumber>());
        using (var writer = new ShapeBinaryWriter(
                   new ShapeFileHeader(
                       roadNodeShapeChangeRecord.Length.Plus(ShapeFileHeader.Length),
                       ShapeType.Point,
                       BoundingBox3D.FromGeometry(pointShapeContent.Shape)),
                   new BinaryWriter(
                       roadNodeShapeChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(roadNodeShapeChangeRecord);
        }

        return roadNodeShapeChangeStream;
    }

    public static MemoryStream CreateEmptyRoadNodeShapeFile(this IFixture fixture)
    {
        var roadNodeShapeChangeStream = new MemoryStream();
        using (var writer = new ShapeBinaryWriter(
                   new ShapeFileHeader(
                       ShapeFileHeader.Length,
                       ShapeType.Point,
                       BoundingBox3D.Empty),
                   new BinaryWriter(
                       roadNodeShapeChangeStream,
                       Encoding.UTF8,
                       true)))
        {
            writer.Write(Array.Empty<ShapeRecord>());
        }

        return roadNodeShapeChangeStream;
    }
}
