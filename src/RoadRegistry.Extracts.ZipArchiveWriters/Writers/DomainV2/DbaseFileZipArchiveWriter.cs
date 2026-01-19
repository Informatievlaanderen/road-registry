namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Dbase;

public class DbaseFileZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly string _filename;
    private readonly IReadOnlyCollection<DbaseRecord> _records;
    private readonly DbaseSchema _schema;

    public DbaseFileZipArchiveWriter(string filename, DbaseSchema schema, IReadOnlyCollection<DbaseRecord> records, Encoding encoding)
    {
        _filename = filename.ThrowIfNull();
        _schema = schema.ThrowIfNull();
        _records = records.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveDataProvider);

        var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
        await dbaseRecordWriter.WriteToArchive(archive, _filename, _schema, _records, cancellationToken);
    }
}
