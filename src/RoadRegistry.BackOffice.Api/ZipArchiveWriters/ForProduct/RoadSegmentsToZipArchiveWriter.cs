namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForProduct
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Product.Schema;
    using Product.Schema.RoadSegments;

    public class RoadSegmentsToZipArchiveWriter : IZipArchiveWriter<ProductContext>
    {
        private readonly string _entryFormat;
        private readonly ZipArchiveWriterOptions _zipArchiveWriterOptions;
        private readonly IStreetNameCache _streetNameCache;
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

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
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter = new DbaseBinaryWriter(dbfHeader, new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                foreach (var batch in context.RoadSegments
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
                        .Where(streetNameId => streetNameId.HasValue)
                        .Select(streetNameId => streetNameId.Value);

                    var cachedStreetNames = await _streetNameCache.GetStreetNamesById(cachedStreetNameIds);

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
                foreach (var data in context.RoadSegments.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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
                foreach (var data in context.RoadSegments.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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
}
