namespace RoadRegistry.BackOffice.FeatureCompare;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;

public abstract class FeatureReader<TDbaseRecord, TFeature>: IFeatureReader<TFeature>
    where TDbaseRecord: DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly DbaseSchema _dbaseSchema;

    protected FeatureReader(Encoding encoding, DbaseSchema dbaseSchema)
    {
        _encoding = encoding;
        _dbaseSchema = dbaseSchema;
    }

    protected abstract TFeature ConvertDbfRecordToFeature(RecordNumber recordNumber, TDbaseRecord dbaseRecord);

    public List<TFeature> Read(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var entry = entries.SingleOrDefault(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
        if (entry is null)
        {
            throw new FileNotFoundException($"File '{fileName}' was not found in zip archive", fileName);
        }

        var records = new List<TFeature>();

        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, _encoding))
        {
            var header = DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));

            if (!header.Schema.Equals(_dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(fileName, _dbaseSchema, header.Schema);
            }

            using (var enumerator = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader))
            {
                while (enumerator.MoveNext())
                {
                    var record = enumerator.Current;
                    if (record != null)
                    {
                        records.Add(ConvertDbfRecordToFeature(enumerator.CurrentRecordNumber, record));
                    }
                }
            }
        }

        return records;
    }
}
