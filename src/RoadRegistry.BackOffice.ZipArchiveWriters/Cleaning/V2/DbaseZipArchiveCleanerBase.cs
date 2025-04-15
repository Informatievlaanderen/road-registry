namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning.V2;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using NetTopologySuite.IO.Esri.Dbf;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;

public abstract class DbaseZipArchiveCleanerBase<TDbaseRecord> : IZipArchiveCleaner
    where TDbaseRecord : DbaseRecord, new()
{
    protected FileEncoding Encoding { get; }

    private readonly DbaseSchema _dbaseSchema;
    private readonly ExtractFileName _fileName;

    protected DbaseZipArchiveCleanerBase(DbaseSchema dbaseSchema, FileEncoding encoding, ExtractFileName fileName)
    {
        _dbaseSchema = dbaseSchema;
        Encoding = encoding;
        _fileName = fileName;
    }

    protected abstract bool FixDataInArchive(ZipArchive archive, IReadOnlyCollection<TDbaseRecord> dbfRecords);

    private IReadOnlyCollection<TDbaseRecord> ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName)
    {
        var dbfFileName = featureType.ToDbaseFileName(fileName);
        var entry = archive.FindEntry(dbfFileName);
        if (entry is null)
        {
            throw new OperationCanceledException();
        }

        var records = new List<TDbaseRecord>();

        using var stream = entry.Open();
        using var copiedStream = stream.CopyToNewMemoryStream();
        using var reader = new DbfReader(copiedStream, Encoding);

        var schema = ReadSchema(reader);
        if (!schema.Equals(_dbaseSchema))
        {
            throw new OperationCanceledException();
        }

        using var enumerator = reader.CreateDbaseRecordEnumerator<TDbaseRecord>();
        while (enumerator.MoveNext())
        {
            var record = enumerator.Current;
            if (record != null)
            {
                records.Add(record);
            }
        }

        return records.AsReadOnly();
    }

    private DbaseSchema ReadSchema(DbfReader reader)
    {
        try
        {
            return reader.Fields.ToDbaseSchema();
        }
        catch
        {
            throw new OperationCanceledException();
        }
    }

    public async Task<CleanResult> CleanAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        try
        {
            var features = ReadFeatures(archive, FeatureType.Change, _fileName);
            if (!features.Any())
            {
                return CleanResult.NoChanges;
            }

            var dataChanged = FixDataInArchive(archive, features);
            if (!dataChanged)
            {
                return CleanResult.NoChanges;
            }

            await Save(archive, features, cancellationToken);
            return CleanResult.Changed;
        }
        catch (OperationCanceledException)
        {
            return CleanResult.NotApplicable;
        }
    }

    private async Task Save(ZipArchive archive, IReadOnlyCollection<TDbaseRecord> dbfRecords, CancellationToken cancellationToken)
    {
        var fileName = FeatureType.Change.ToDbaseFileName(_fileName);

        var entry = archive.FindEntry(fileName);
        entry?.Delete();

        var writer = new RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2.DbaseRecordWriter(Encoding);
        await writer.WriteToArchive(archive, fileName, _dbaseSchema, dbfRecords, cancellationToken);
    }
}
