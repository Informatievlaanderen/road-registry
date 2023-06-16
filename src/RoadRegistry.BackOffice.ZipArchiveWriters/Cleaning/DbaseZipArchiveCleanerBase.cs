namespace RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using FeatureCompare;

public abstract class DbaseZipArchiveCleanerBase<TDbaseRecord> : IZipArchiveCleaner
    where TDbaseRecord : DbaseRecord, new()
{
    protected Encoding Encoding { get; }

    private readonly DbaseSchema _dbaseSchema;
    private readonly ExtractFileName _fileName;

    protected DbaseZipArchiveCleanerBase(DbaseSchema dbaseSchema, Encoding encoding, ExtractFileName fileName)
    {
        _dbaseSchema = dbaseSchema;
        Encoding = encoding;
        _fileName = fileName;
    }

    protected abstract bool FixDataInArchive(ZipArchive archive, IReadOnlyCollection<TDbaseRecord> dbfRecords);

    private IReadOnlyCollection<TDbaseRecord> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        var dbfFileName = featureType.GetDbfFileName(fileName);
        var entry = entries.SingleOrDefault(x => x.Name.Equals(dbfFileName, StringComparison.InvariantCultureIgnoreCase));
        if (entry is null)
        {
            return null;
        }

        var records = new List<TDbaseRecord>();

        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, Encoding))
        {
            var header = ReadHeader(reader);
            if (header is null || !header.Schema.Equals(_dbaseSchema))
            {
                return null;
            }

            using (var enumerator = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader))
            {
                while (enumerator.MoveNext())
                {
                    var record = enumerator.Current;
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
            }
        }

        return records.AsReadOnly();
    }

    private DbaseFileHeader ReadHeader(BinaryReader reader)
    {
        try
        {
            return DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));
        }
        catch
        {
            return null;
        }
    }

    public async Task<CleanResult> CleanAsync(ZipArchive archive, CancellationToken cancellationToken)
    {
        var features = ReadFeatures(archive.Entries, FeatureType.Change, _fileName);
        if (features is null)
        {
            return CleanResult.NotApplicable;
        }
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


    private async Task Save(ZipArchive archive, IReadOnlyCollection<TDbaseRecord> dbfRecords, CancellationToken cancellationToken)
    {
        var fileName = FeatureType.Change.GetDbfFileName(_fileName);
        archive.Entries
            .Single(x => string.Equals(x.Name, fileName, StringComparison.InvariantCultureIgnoreCase))
            .Delete();
        
        await SaveDbaseRecords(archive, fileName, dbfRecords, cancellationToken);
    }

    private async Task SaveDbaseRecords(ZipArchive archive,
        string fileName,
        IReadOnlyCollection<TDbaseRecord> records,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(records);

        var dbfEntry = archive.CreateEntry(fileName);
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(records.Count),
            _dbaseSchema
        );

        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, Encoding, true)))
        {
            foreach (var dbfRecord in records)
            {
                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
