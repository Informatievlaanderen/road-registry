namespace RoadRegistry.BackOffice.FeatureCompare.V1;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;

public abstract class ZipArchiveDbaseFeatureReader<TDbaseRecord, TFeature> : IZipArchiveDbaseFeatureReader<TFeature>
    where TDbaseRecord : DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly DbaseSchema _dbaseSchema;
    private readonly bool _treatHasNoDbaseRecordsAsError;

    protected ZipArchiveDbaseFeatureReader(Encoding encoding, DbaseSchema dbaseSchema, bool treatHasNoDbaseRecordsAsError = false)
    {
        _encoding = encoding;
        _dbaseSchema = dbaseSchema;
        _treatHasNoDbaseRecordsAsError = treatHasNoDbaseRecordsAsError;
    }

    protected abstract (TFeature, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, TDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context);

    public (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var problems = ZipArchiveProblems.None;

        var dbfFileName = featureType.ToDbaseFileName(fileName);
        var entry = archive.FindEntry(dbfFileName);
        if (entry is null)
        {
            problems += problems.RequiredFileMissing(dbfFileName);

            return (new List<TFeature>(), problems);
        }

        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, _encoding))
        {
            try
            {
                var header = ReadHeader(reader);

                if (!header.Schema.Equals(_dbaseSchema))
                {
                    throw new DbaseSchemaMismatchException(dbfFileName, _dbaseSchema, header.Schema);
                }

                using (var records = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader))
                {
                    return ReadFeatures(featureType, fileName, entry, records, context);
                }
            }
            catch (DbaseHeaderFormatException ex)
            {
                problems += entry.HasDbaseHeaderFormatError(ex.InnerException);
            }
            catch (DbaseSchemaMismatchException ex)
            {
                problems += entry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
            }
        }

        return (new List<TFeature>(), problems);
    }

    protected virtual (List<TFeature>, ZipArchiveProblems) ReadFeatures(FeatureType featureType, ExtractFileName fileName, ZipArchiveEntry entry, IDbaseRecordEnumerator<TDbaseRecord> records, ZipArchiveFeatureReaderContext context)
    {
        var problems = ZipArchiveProblems.None;
        var features = new List<TFeature>();

        try
        {
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var record = records.Current;
                    if (record != null)
                    {
                        var (feature, recordProblems) = ConvertToFeature(featureType, fileName, records.CurrentRecordNumber, record, context);

                        problems += recordProblems;
                        features.Add(feature);
                    }

                    moved = records.MoveNext();
                }
            }
            else
            {
                problems += entry.HasNoDbaseRecords(_treatHasNoDbaseRecordsAsError);
            }
        }
        catch (Exception exception)
        {
            problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (features, problems);
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
