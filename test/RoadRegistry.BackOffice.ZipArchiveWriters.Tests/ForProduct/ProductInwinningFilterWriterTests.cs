namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.ForProduct;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using Framework.Containers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Product.Schema;
using Product.Schema.GradeSeparatedJunctions;
using Product.Schema.RoadNodes;
using Product.Schema.RoadSegments;
using RoadRegistry.Editor.Schema.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schemas.ExtractV1.GradeSeparatedJuntions;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadNodes;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Extracts.ZipArchiveWriters;
using ZipArchiveWriters.ForProduct;

[Collection(nameof(SqlServerCollection))]
public class ProductInwinningFilterWriterTests
{
    private static readonly int RoadNodeShapeLength = PointShapeContent.Length.Plus(ShapeRecord.HeaderLength).ToInt32();

    private readonly SqlServer _fixture;
    private readonly FileEncoding _fileEncoding;

    public ProductInwinningFilterWriterTests(SqlServer fixture, FileEncoding fileEncoding)
    {
        _fixture = fixture;
        _fileEncoding = fileEncoding;
    }

    [Fact]
    public async Task RoadNodesLeavesOutTheExcludedRoadNodes()
    {
        var context = await CreateContextAsync();
        await context.RoadNodes.AddRangeAsync(RoadNode(1), RoadNode(2), RoadNode(3));
        await context.RoadNetworkInfo.AddAsync(new RoadNetworkInfo { CompletedImport = true, TotalRoadNodeShapeLength = 3 * RoadNodeShapeLength });
        await context.SaveChangesAsync();

        var sut = new RoadNodesToZipArchiveWriter("{0}", _fixture.MemoryStreamManager, _fileEncoding, new ProductInwinningFilter([], [2]));

        await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(archive =>
            {
                ReadDbaseRecords<RoadNodeDbaseRecord>(archive, "Wegknoop.dbf").Select(x => x.WK_OIDN.Value).Should().Equal(1, 3);
                ReadShapeFileLength(archive, "Wegknoop.shp").Should().Be(new WordLength(2 * RoadNodeShapeLength));
                ReadShapeFileLength(archive, "Wegknoop.shx").Should().Be(ShapeFileHeader.Length.Plus(ShapeIndexRecord.Length.Times(2)));
            });
    }

    [Fact]
    public async Task RoadSegmentsLeavesOutTheExcludedRoadSegments()
    {
        var context = await CreateContextAsync();
        var (roadSegment1, shapeLength1) = RoadSegment(1, length: 10);
        var (roadSegment2, shapeLength2) = RoadSegment(2, length: 20);
        var (roadSegment3, shapeLength3) = RoadSegment(3, length: 30);
        await context.RoadSegments.AddRangeAsync(roadSegment1, roadSegment2, roadSegment3);
        await context.RoadNetworkInfo.AddAsync(new RoadNetworkInfo { CompletedImport = true, TotalRoadSegmentShapeLength = shapeLength1 + shapeLength2 + shapeLength3 });
        await context.SaveChangesAsync();

        var sut = new RoadSegmentsToZipArchiveWriter("{0}", new ZipArchiveWriterOptions(), _fixture.StreetNameCache, _fixture.MemoryStreamManager, _fileEncoding, new ProductInwinningFilter([2], []));

        await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(archive =>
            {
                ReadDbaseRecords<RoadSegmentDbaseRecord>(archive, "Wegsegment.dbf").Select(x => x.WS_OIDN.Value).Should().Equal(1, 3);
                ReadShapeFileLength(archive, "Wegsegment.shp").Should().Be(new WordLength(shapeLength1 + shapeLength3));
                ReadShapeFileLength(archive, "Wegsegment.shx").Should().Be(ShapeFileHeader.Length.Plus(ShapeIndexRecord.Length.Times(2)));
            });
    }

    [Fact]
    public async Task RoadSegmentAttributesLeaveOutTheAttributesOfExcludedRoadSegments()
    {
        var context = await CreateContextAsync();
        await context.RoadSegmentLaneAttributes.AddRangeAsync(
            LaneAttribute(id: 1, roadSegmentId: 1),
            LaneAttribute(id: 2, roadSegmentId: 2),
            LaneAttribute(id: 3, roadSegmentId: 2),
            LaneAttribute(id: 4, roadSegmentId: 3));
        await context.SaveChangesAsync();

        var sut = new RoadSegmentLaneAttributesToZipArchiveWriter("{0}", _fixture.MemoryStreamManager, _fileEncoding, new ProductInwinningFilter([2], []));

        await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(archive =>
            {
                ReadDbaseRecords<RoadSegmentLaneAttributeDbaseRecord>(archive, "AttRijstroken.dbf").Select(x => x.RS_OIDN.Value).Should().Equal(1, 4);
            });
    }

    [Fact]
    public async Task GradeSeparatedJunctionsLeaveOutTheJunctionsOfExcludedRoadSegments()
    {
        var context = await CreateContextAsync();
        await context.GradeSeparatedJunctions.AddRangeAsync(
            GradeSeparatedJunction(id: 1, upperRoadSegmentId: 5, lowerRoadSegmentId: 6),
            GradeSeparatedJunction(id: 2, upperRoadSegmentId: 7, lowerRoadSegmentId: 5),
            GradeSeparatedJunction(id: 3, upperRoadSegmentId: 7, lowerRoadSegmentId: 8));
        await context.SaveChangesAsync();

        var sut = new GradeSeparatedJunctionArchiveWriter("{0}", _fixture.MemoryStreamManager, _fileEncoding, new ProductInwinningFilter([5], []));

        await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(archive =>
            {
                ReadDbaseRecords<GradeSeparatedJunctionDbaseRecord>(archive, "RltOgkruising.dbf").Select(x => x.OK_OIDN.Value).Should().Equal(3);
            });
    }

    private async Task<ProductContext> CreateContextAsync()
    {
        var database = await _fixture.CreateDatabaseAsync();
        return await _fixture.CreateProductContextAsync(database);
    }

    private RoadNodeRecord RoadNode(int id)
    {
        var shapeContent = new PointShapeContent(GeometryTranslator.FromGeometryPoint(new NetTopologySuite.Geometries.Point(id, id)));
        return new RoadNodeRecord
        {
            Id = id,
            ShapeRecordContent = shapeContent.ToBytes(_fixture.MemoryStreamManager, _fileEncoding),
            ShapeRecordContentLength = shapeContent.ContentLength.ToInt32(),
            DbaseRecord = new RoadNodeDbaseRecord { WK_OIDN = { Value = id } }.ToBytes(_fixture.MemoryStreamManager, _fileEncoding),
            BoundingBox = RoadNodeBoundingBox.From(shapeContent.Shape)
        };
    }

    private (RoadSegmentRecord Record, int ShapeLength) RoadSegment(int id, double length)
    {
        var multiLineString = new MultiLineString([
            new LineString(new CoordinateArraySequence([new CoordinateM(0, id, 0), new CoordinateM(length, id, length)], 3, 1), GeometryFactory.Default)
        ], GeometryFactory.Default);
        var shapeContent = new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(multiLineString));

        var record = new RoadSegmentRecord
        {
            Id = id,
            StartNodeId = id * 10,
            EndNodeId = id * 10 + 1,
            ShapeRecordContent = shapeContent.ToBytes(_fixture.MemoryStreamManager, _fileEncoding),
            ShapeRecordContentLength = shapeContent.ContentLength.ToInt32(),
            DbaseRecord = new RoadSegmentDbaseRecord { WS_OIDN = { Value = id } }.ToBytes(_fixture.MemoryStreamManager, _fileEncoding)
        }.WithBoundingBox(RoadSegmentBoundingBox.From(shapeContent.Shape));

        return (record, shapeContent.ContentLength.Plus(ShapeRecord.HeaderLength).ToInt32());
    }

    private RoadSegmentLaneAttributeRecord LaneAttribute(int id, int roadSegmentId)
    {
        return new RoadSegmentLaneAttributeRecord
        {
            Id = id,
            RoadSegmentId = roadSegmentId,
            DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord { RS_OIDN = { Value = id }, WS_OIDN = { Value = roadSegmentId } }.ToBytes(_fixture.MemoryStreamManager, _fileEncoding)
        };
    }

    private GradeSeparatedJunctionRecord GradeSeparatedJunction(int id, int upperRoadSegmentId, int lowerRoadSegmentId)
    {
        return new GradeSeparatedJunctionRecord
        {
            Id = id,
            DbaseRecord = new GradeSeparatedJunctionDbaseRecord
            {
                OK_OIDN = { Value = id },
                BO_WS_OIDN = { Value = upperRoadSegmentId },
                ON_WS_OIDN = { Value = lowerRoadSegmentId }
            }.ToBytes(_fixture.MemoryStreamManager, _fileEncoding)
        };
    }

    private IReadOnlyList<TRecord> ReadDbaseRecords<TRecord>(ZipArchive archive, string entryName)
        where TRecord : DbaseRecord, new()
    {
        using var stream = archive.GetEntry(entryName)!.Open();
        using var reader = new BinaryReader(stream, _fileEncoding);

        var header = DbaseFileHeader.Read(reader);
        var records = new List<TRecord>();
        for (var i = 0; i < header.RecordCount.ToInt32(); i++)
        {
            var record = new TRecord();
            record.Read(reader);
            records.Add(record);
        }

        return records;
    }

    private WordLength ReadShapeFileLength(ZipArchive archive, string entryName)
    {
        using var stream = archive.GetEntry(entryName)!.Open();
        using var reader = new BinaryReader(stream, _fileEncoding);

        return ShapeFileHeader.Read(reader).FileLength;
    }
}
