namespace RoadRegistry.Extracts.Infrastructure.Dbase;

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using NetTopologySuite.IO.Esri.Dbf;
using RoadRegistry.Extensions;

public class DbaseRecordWriter
{
    private readonly Encoding _encoding;

    public DbaseRecordWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public Task WriteToArchive(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        DbaseSchema dbaseSchema,
        IEnumerable<DbaseRecord> dbaseRecords,
        CancellationToken cancellationToken)
    {
        return WriteToArchive(archive, fileName.ToDbaseFileName(featureType), dbaseSchema, dbaseRecords, cancellationToken);
    }

    public async Task WriteToArchive(
        ZipArchive archive,
        string fileName,
        DbaseSchema dbaseSchema,
        IEnumerable<DbaseRecord> dbaseRecords,
        CancellationToken cancellationToken)
    {
        var dbfStream = WriteToDbfStream(dbaseSchema, dbaseRecords);

        var dbfEntry = archive.CreateEntry(fileName);
        await dbfEntry.CopyFrom(dbfStream, cancellationToken);
    }

    public MemoryStream WriteToDbfStream(DbaseSchema dbaseSchema, IEnumerable<DbaseRecord> dbaseRecords)
    {
        var dbfFields = dbaseSchema.ToDbfFields();

        var dbfStream = new MemoryStream();
        using var dbfWriter = new DbfWriter(dbfStream, dbfFields, _encoding);

        foreach (var dbaseRecord in dbaseRecords)
        {
            var attributes = dbaseRecord
                .ToAttributesTable()
                .ToDictionary(x => x.Key, x => x.Value);
            dbfWriter.Write(attributes);
        }

        return dbfStream;
    }
}
