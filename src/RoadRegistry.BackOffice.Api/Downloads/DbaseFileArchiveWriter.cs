namespace RoadRegistry.Api.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class DbaseFileArchiveWriter<TDbaseRecord, TDbaseSchema>
        where TDbaseRecord : DbaseRecord
        where TDbaseSchema : DbaseSchema, new()
    {
        private readonly string _filename;
        private readonly Encoding _encoding;

        public DbaseFileArchiveWriter(string filename, Encoding encoding)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, IReadOnlyCollection<TDbaseRecord> dbfRecords, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (dbfRecords == null) throw new ArgumentNullException(nameof(dbfRecords));
            
            var dbfEntry = archive.CreateEntry(_filename);
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(dbfRecords.Count),
                new TDbaseSchema()
            );
            using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                foreach (var dbfRecord in dbfRecords)
                {
                    dbfWriter.Write(dbfRecord);
                }
                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}