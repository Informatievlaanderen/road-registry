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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Product.Schema;
    using Product.Schema.RoadSegments;

    public class RoadSegmentEuropeanRoadAttributesToZipArchiveWriter : IZipArchiveWriter<ProductContext>
    {
        private readonly string _entryFormat;
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(string entryFormat, RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            _entryFormat = entryFormat ?? throw new ArgumentNullException(nameof(entryFormat));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var count = await context.RoadSegmentEuropeanRoadAttributes.CountAsync(cancellationToken);
            var dbfEntry = archive.CreateEntry("AttEuropweg.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(count),
                RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord();
                foreach (var data in context.RoadSegmentEuropeanRoadAttributes.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
