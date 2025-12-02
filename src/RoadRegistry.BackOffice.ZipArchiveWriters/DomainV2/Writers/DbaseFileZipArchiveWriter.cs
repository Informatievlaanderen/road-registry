namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2.Writers;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Dbase.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extensions;

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
