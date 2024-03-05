namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts.Dbase.RoadSegments;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Product.Schema;

public class RoadSegmentsToZipArchiveWriter : IZipArchiveWriter<ProductContext>
{
    private readonly Encoding _encoding;
    private readonly string _entryFormat;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IStreetNameCache _streetNameCache;
    private readonly ZipArchiveWriterOptions _zipArchiveWriterOptions;

    public RoadSegmentsToZipArchiveWriter(
        string entryFormat,
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        _entryFormat = entryFormat ?? throw new ArgumentNullException(nameof(entryFormat));
        _zipArchiveWriterOptions = zipArchiveWriterOptions ?? throw new ArgumentNullException(nameof(zipArchiveWriterOptions));
        _streetNameCache = streetNameCache ?? throw new ArgumentNullException(nameof(streetNameCache));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var count = await context.RoadSegments.CountAsync(cancellationToken);
        var dbfEntry = archive.CreateEntry(string.Format(_entryFormat, "Wegsegment.dbf"));
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(count),
            RoadSegmentDbaseRecord.Schema
        );

        var cachedStreetNames = new Dictionary<int, string>();
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter = new DbaseBinaryWriter(dbfHeader, new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            foreach (var batch in context.RoadSegments
                         .OrderBy(x => x.Id)
                         .Select(x => x.DbaseRecord)
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
                    .Where(streetNameId => streetNameId.HasValue)
                    .Select(streetNameId => streetNameId.Value)
                    .Where(streetNameId => !cachedStreetNames.ContainsKey(streetNameId));

                var missingCachedStreetNames = await _streetNameCache.GetStreetNamesById(cachedStreetNameIds, cancellationToken);
                foreach (var item in missingCachedStreetNames)
                {
                    cachedStreetNames.Add(item.Key, item.Value);
                }

                foreach (var dbfRecord in dbfRecords)
                {
                    if (dbfRecord.LSTRNMID.Value.HasValue && cachedStreetNames.TryGetValue(dbfRecord.LSTRNMID.Value.Value, out var leftStreetName))
                    {
                        dbfRecord.LSTRNM.Value = leftStreetName;
                    }

                    if (dbfRecord.RSTRNMID.Value.HasValue && cachedStreetNames.TryGetValue(dbfRecord.RSTRNMID.Value.Value, out var rightStreetName))
                    {
                        dbfRecord.RSTRNM.Value = rightStreetName;
                    }

                    dbfWriter.Write(dbfRecord);
                }
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }

        var shpBoundingBox = count > 0
            ? (await context.RoadSegmentBoundingBox.SingleAsync(cancellationToken)).ToBoundingBox3D()
            : BoundingBox3D.Empty;

        var info = await context.RoadNetworkInfo.SingleAsync(cancellationToken);

        var shpEntry = archive.CreateEntry(string.Format(_entryFormat, "Wegsegment.shp"));
        var shpHeader = new ShapeFileHeader(
            new WordLength(info.TotalRoadSegmentShapeLength),
            ShapeType.PolyLineM,
            shpBoundingBox);
        await using (var shpEntryStream = shpEntry.Open())
        using (var shpWriter =
               new ShapeBinaryWriter(
                   shpHeader,
                   new BinaryWriter(shpEntryStream, _encoding, true)))
        {
            var number = RecordNumber.Initial;
            foreach (var data in context.RoadSegments.OrderBy(x => x.Id).Select(x => x.ShapeRecordContent))
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

        var shxEntry = archive.CreateEntry(string.Format(_entryFormat, "Wegsegment.shx"));
        var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(count));
        await using (var shxEntryStream = shxEntry.Open())
        using (var shxWriter =
               new ShapeIndexBinaryWriter(
                   shxHeader,
                   new BinaryWriter(shxEntryStream, _encoding, true)))
        {
            var offset = ShapeIndexRecord.InitialOffset;
            var number = RecordNumber.Initial;
            foreach (var data in context.RoadSegments.OrderBy(x => x.Id).Select(x => x.ShapeRecordContent))
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

        await archive.CreateCpgEntry(string.Format(_entryFormat, "Wegsegment.cpg"), _encoding, cancellationToken);
    }
}
