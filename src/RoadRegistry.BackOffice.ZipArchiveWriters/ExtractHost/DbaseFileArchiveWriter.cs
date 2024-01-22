namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;

public class DbaseFileArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    private readonly Encoding _encoding;
    private readonly string _filename;
    private readonly IReadOnlyCollection<DbaseRecord> _records;
    private readonly DbaseSchema _schema;

    public DbaseFileArchiveWriter(string filename, DbaseSchema schema, IReadOnlyCollection<DbaseRecord> records, Encoding encoding)
    {
        _filename = filename.ThrowIfNull();
        _schema = schema.ThrowIfNull();
        _records = records.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var dbfEntry = archive.CreateEntry(_filename);
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(_records.Count),
            _schema
        );
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            foreach (var dbfRecord in _records) dbfWriter.Write(dbfRecord);
            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
