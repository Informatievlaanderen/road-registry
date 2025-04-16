namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning.V2;

using System.IO.Compression;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.V2;
using Exceptions;
using Extracts;

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
        try
        {
            using var dbase = new DbaseRecordReader(Encoding)
                .ReadFromArchive<TDbaseRecord>(archive, fileName, featureType, _dbaseSchema);

            var records = new List<TDbaseRecord>();

            using var enumerator = dbase.RecordEnumerator;
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
        catch (ZipArchiveValidationException)
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

        var writer = new DbaseRecordWriter(Encoding);
        await writer.WriteToArchive(archive, fileName, _dbaseSchema, dbfRecords, cancellationToken);
    }
}
