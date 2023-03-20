namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Editor.Schema.RoadSegments;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Microsoft.IO;
using NetTopologySuite.Geometries;

public class IntegrationRoadSegmentsToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IStreetNameCache _streetNameCache;
    private readonly ZipArchiveWriterOptions _zipArchiveWriterOptions;

    public IntegrationRoadSegmentsToZipArchiveWriter(
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
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (context == null) throw new ArgumentNullException(nameof(context));

        const int integrationBufferInMeters = 350;

        var segmentsInContour = await context.RoadSegments
            .ToListWithPolygonials(request.Contour,
                (dbSet, polygon) => dbSet.InsideContour(polygon),
                x => x.Id,
                cancellationToken);

        var segmentsInIntegrationBuffer = await context.RoadSegments
            .ToListWithPolygonials(request.Contour,
                (dbSet, polygon) => dbSet.InsideContour((IPolygonal)((Geometry)polygon).Buffer(integrationBufferInMeters)),
                x => x.Id,
                cancellationToken);

        var integrationSegments = segmentsInIntegrationBuffer.Except(segmentsInContour, new RoadSegmentRecordEqualityComparerById()).ToList();

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
                         .OrderBy(_ => _.Id)
                         .Select(_ => _.DbaseRecord)
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
            (box, record) => box.ExpandWith(record.BoundingBox.ToBoundingBox3D()));

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
            foreach (var data in integrationSegments.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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
            foreach (var data in integrationSegments.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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
    }
}
