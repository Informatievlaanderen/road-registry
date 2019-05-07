namespace RoadRegistry.Api.ZipArchiveWriters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Schema;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class DbaseFileArchiveWriter : IZipArchiveWriter
    {
        private readonly string _filename;
        private readonly DbaseSchema _schema;
        private readonly IReadOnlyCollection<DbaseRecord> _records;
        private readonly Encoding _encoding;

        public DbaseFileArchiveWriter(string filename, DbaseSchema schema, IReadOnlyCollection<DbaseRecord> records, Encoding encoding)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _records = records ?? throw new ArgumentNullException(nameof(records));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, ShapeContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var dbfEntry = archive.CreateEntry(_filename);
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(_records.Count),
                _schema
            );
            using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                foreach (var dbfRecord in _records)
                {
                    dbfWriter.Write(dbfRecord);
                }
                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
