namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Editor.Schema.RoadNodes;
using Editor.Schema.RoadSegments;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadNodes;
using Extracts.Dbase.RoadSegments;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using System.IO.Compression;
using System.Text;
using FeatureCompare;

public class IntegrationToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IStreetNameCache _streetNameCache;
    private readonly ZipArchiveWriterOptions _zipArchiveWriterOptions;

    private const int IntegrationBufferInMeters = 350;

    public IntegrationToZipArchiveWriter(
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        _zipArchiveWriterOptions = zipArchiveWriterOptions ?? throw new ArgumentNullException(nameof(zipArchiveWriterOptions));
        _streetNameCache = streetNameCache ?? throw new ArgumentNullException(nameof(streetNameCache));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
        EditorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var segmentsInContour = await context.RoadSegments
            .ToListWithPolygonials(request.Contour,
                (dbSet, polygon) => dbSet.InsideContour(polygon),
                x => x.Id,
                cancellationToken);
        var nodesInContour = await context.RoadNodes
            .ToListWithPolygonials(request.Contour,
                (dbSet, polygon) => dbSet.InsideContour(polygon),
                x => x.Id,
                cancellationToken);

        // segments integration
        var integrationBufferedSegmentsGeometries = segmentsInContour.Select(x => x.Geometry.Buffer(IntegrationBufferInMeters)).ToList();

        var integrationSegments = new List<RoadSegmentRecord>();
        var integrationNodes = new List<RoadNodeRecord>();

        if (integrationBufferedSegmentsGeometries.Any())
        {
            var integrationBufferedContourGeometry = WellKnownGeometryFactories.Default
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            var segmentsInIntegrationBuffer = await context.RoadSegments
                .InsideContour((IPolygonal)integrationBufferedContourGeometry)
                .ToListAsync(cancellationToken);

            integrationSegments = segmentsInIntegrationBuffer.Except(segmentsInContour, new RoadSegmentRecordEqualityComparerById()).ToList();
            integrationSegments = integrationSegments.Where(integrationSegment => { return integrationBufferedSegmentsGeometries.Any(segmentBufferedGeometry => segmentBufferedGeometry.Intersects(integrationSegment.Geometry)); })
                .ToList();

            // nodes integration
            var nodesInIntegrationBuffer = await context.RoadNodes
                .InsideContour((IPolygonal)integrationBufferedContourGeometry)
                .ToListAsync(cancellationToken);


            var integrationNodeIds = integrationSegments.SelectMany(segment => new[] { segment.StartNodeId, segment.EndNodeId }).Distinct().ToList();
            integrationNodes = nodesInIntegrationBuffer.Where(integrationNode => integrationNodeIds.Contains(integrationNode.Id))
                .ToList();
            integrationNodes = integrationNodes.Except(nodesInContour, new RoadNodeRecordEqualityComparerById()).ToList();
        }
        await WriteRoadSegments();
        await WriteRoadNodes();

        async Task WriteRoadSegments()
        {
            var dbfEntry = archive.CreateEntry("iWegsegment.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(integrationSegments.Count),
                RoadSegmentDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter = new DbaseBinaryWriter(dbfHeader, new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                foreach (var batch in integrationSegments
                             .OrderBy(record => record.Id)
                             .Select(record => record.DbaseRecord)
                             .AsEnumerable()
                             .Batch(_zipArchiveWriterOptions.RoadSegmentBatchSize))
                {
                    var dbfRecords = batch
                        .Select(x =>
                        {
                            var dbfRecord = new RoadSegmentDbaseRecord();
                            dbfRecord.FromBytes(x, _manager, _encoding);
                            return dbfRecord;
                        })
                        .ToList();

                    var cachedStreetNameIds = dbfRecords
                        .Select(record => record.LSTRNMID.Value)
                        .Union(dbfRecords.Select(record => record.RSTRNMID.Value))
                        .Where(streetNameId => streetNameId.HasValue && streetNameId.Value >= 0)
                        .Select(streetNameId => streetNameId.Value);

                    var cachedStreetNames = await _streetNameCache.GetStreetNamesById(cachedStreetNameIds, cancellationToken);

                    foreach (var dbfRecord in dbfRecords)
                    {
                        if (dbfRecord.LSTRNMID.Value.HasValue && cachedStreetNames.ContainsKey(dbfRecord.LSTRNMID.Value.Value))
                            dbfRecord.LSTRNM.Value = cachedStreetNames[dbfRecord.LSTRNMID.Value.Value];

                        if (dbfRecord.RSTRNMID.Value.HasValue && cachedStreetNames.ContainsKey(dbfRecord.RSTRNMID.Value.Value))
                            dbfRecord.RSTRNM.Value = cachedStreetNames[dbfRecord.RSTRNMID.Value.Value];

                        dbfWriter.Write(dbfRecord);
                    }
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }

            var shpBoundingBox = integrationSegments.Aggregate(
                BoundingBox3D.Empty,
                (box, record) => box.ExpandWith(record.GetBoundingBox().ToBoundingBox3D()));

            var shpEntry = archive.CreateEntry("iWegsegment.shp");
            var shpHeader = new ShapeFileHeader(
                new WordLength(
                    integrationSegments.Aggregate(0, (length, record) => length + record.ShapeRecordContentLength)),
                ShapeType.PolyLineM,
                shpBoundingBox);
            await using (var shpEntryStream = shpEntry.Open())
            using (var shpWriter =
                   new ShapeBinaryWriter(
                       shpHeader,
                       new BinaryWriter(shpEntryStream, _encoding, true)))
            {
                var number = RecordNumber.Initial;
                foreach (var data in integrationSegments.OrderBy(record => record.Id).Select(record => record.ShapeRecordContent))
                {
                    shpWriter.Write(
                        ShapeContentFactory
                            .FromBytes(data, _manager, _encoding)
                            .RecordAs(number)
                    );
                    number = number.Next();
                }

                shpWriter.Writer.Flush();
                await shpEntryStream.FlushAsync(cancellationToken);
            }

            var shxEntry = archive.CreateEntry("iWegsegment.shx");
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(integrationSegments.Count));
            await using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                   new ShapeIndexBinaryWriter(
                       shxHeader,
                       new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                var offset = ShapeIndexRecord.InitialOffset;
                var number = RecordNumber.Initial;
                foreach (var data in integrationSegments.OrderBy(record => record.Id).Select(record => record.ShapeRecordContent))
                {
                    var shpRecord = ShapeContentFactory
                        .FromBytes(data, _manager, _encoding)
                        .RecordAs(number);
                    shxWriter.Write(shpRecord.IndexAt(offset));
                    number = number.Next();
                    offset = offset.Plus(shpRecord.Length);
                }

                shxWriter.Writer.Flush();
                await shxEntryStream.FlushAsync(cancellationToken);
            }

            await archive.CreateCpgEntry("iWegsegment.cpg", _encoding, cancellationToken);
            await archive.CreateProjectionEntry(FeatureType.Integration.ToProjectionFileName(ExtractFileName.Wegsegment), _encoding, cancellationToken);
        }

        async Task WriteRoadNodes()
        {
            var dbfEntry = archive.CreateEntry("iWegknoop.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(integrationNodes.Count),
                RoadNodeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                   new DbaseBinaryWriter(
                       dbfHeader,
                       new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadNodeDbaseRecord();
                foreach (var data in integrationNodes.OrderBy(record => record.Id).Select(record => record.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }

            var shpBoundingBox =
                integrationNodes.Aggregate(
                    BoundingBox3D.Empty,
                    (box, record) => box.ExpandWith(record.GetBoundingBox().ToBoundingBox3D()));

            var shpEntry = archive.CreateEntry("iWegknoop.shp");
            var shpHeader = new ShapeFileHeader(
                new WordLength(
                    integrationNodes.Aggregate(0, (length, record) => length + record.ShapeRecordContentLength)),
                ShapeType.Point,
                shpBoundingBox);
            await using (var shpEntryStream = shpEntry.Open())
            using (var shpWriter =
                   new ShapeBinaryWriter(
                       shpHeader,
                       new BinaryWriter(shpEntryStream, _encoding, true)))
            {
                var number = RecordNumber.Initial;
                foreach (var data in integrationNodes.OrderBy(record => record.Id).Select(record => record.ShapeRecordContent))
                {
                    shpWriter.Write(
                        ShapeContentFactory
                            .FromBytes(data, _manager, _encoding)
                            .RecordAs(number)
                    );
                    number = number.Next();
                }

                shpWriter.Writer.Flush();
                await shpEntryStream.FlushAsync(cancellationToken);
            }

            var shxEntry = archive.CreateEntry("iWegknoop.shx");
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(integrationNodes.Count));
            await using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                   new ShapeIndexBinaryWriter(
                       shxHeader,
                       new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                var offset = ShapeIndexRecord.InitialOffset;
                var number = RecordNumber.Initial;
                foreach (var data in integrationNodes.OrderBy(record => record.Id).Select(record => record.ShapeRecordContent))
                {
                    var shpRecord = ShapeContentFactory
                        .FromBytes(data, _manager, _encoding)
                        .RecordAs(number);
                    shxWriter.Write(shpRecord.IndexAt(offset));
                    number = number.Next();
                    offset = offset.Plus(shpRecord.Length);
                }

                shxWriter.Writer.Flush();
                await shxEntryStream.FlushAsync(cancellationToken);
            }

            await archive.CreateCpgEntry("iWegknoop.cpg", _encoding, cancellationToken);
            await archive.CreateProjectionEntry(FeatureType.Integration.ToProjectionFileName(ExtractFileName.Wegknoop), _encoding, cancellationToken);
        }
    }
}
