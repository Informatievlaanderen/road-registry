namespace RoadRegistry.BackOffice.FeatureCompare;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using RoadRegistry.BackOffice.Uploads;

public abstract class DbaseFeatureReader<TDbaseRecord, TFeature>: IFeatureReader<TFeature>
    where TDbaseRecord: DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly DbaseSchema _dbaseSchema;

    protected DbaseFeatureReader(Encoding encoding, DbaseSchema dbaseSchema)
    {
        _encoding = encoding;
        _dbaseSchema = dbaseSchema;
    }

    protected abstract TFeature ConvertDbfRecordToFeature(RecordNumber recordNumber, TDbaseRecord dbaseRecord);

    public List<TFeature> Read(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        var dbfFileName = featureType.GetDbfFileName(fileName);
        var entry = entries.SingleOrDefault(x => x.Name.Equals(dbfFileName, StringComparison.InvariantCultureIgnoreCase));
        if (entry is null)
        {
            throw new FileNotFoundException($"File '{dbfFileName}' was not found in zip archive", dbfFileName);
        }

        var records = new List<TFeature>();

        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, _encoding))
        {
            var header = ReadHeader(reader);

            if (!header.Schema.Equals(_dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(dbfFileName, _dbaseSchema, header.Schema);
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

    private DbaseFileHeader ReadHeader(BinaryReader reader)
    {
        try
        {
            return DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));
        }
        catch (Exception exception)
        {
            throw new DbaseHeaderFormatException(exception);
        }
    }
}
