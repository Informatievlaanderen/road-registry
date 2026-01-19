namespace RoadRegistry.Extracts.FeatureCompare.V3;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Dbase;
using Uploads;

public abstract class ZipArchiveDbaseFeatureReader<TDbaseRecord, TFeature> : IZipArchiveFeatureReader<TFeature>
    where TDbaseRecord : DbaseRecord, new()
{
    protected readonly ExtractFileName FileName;
    private readonly Encoding _encoding;
    private readonly DbaseSchema _dbaseSchema;
    private readonly bool _treatHasNoDbaseRecordsAsError;

    protected ZipArchiveDbaseFeatureReader(Encoding encoding, ExtractFileName fileName, DbaseSchema dbaseSchema, bool treatHasNoDbaseRecordsAsError = false)
    {
        FileName = fileName;
        _encoding = encoding;
        _dbaseSchema = dbaseSchema;
        _treatHasNoDbaseRecordsAsError = treatHasNoDbaseRecordsAsError;
    }

    protected abstract (TFeature, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, TDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context);

    public (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        try
        {
            using var dbase = new DbaseRecordReader(_encoding)
                .ReadFromArchive<TDbaseRecord>(archive, FileName, featureType, _dbaseSchema);

            return ReadFeatures(featureType, dbase.DbaseEntry, dbase.RecordEnumerator, context);
        }
        catch (ZipArchiveValidationException ex)
        {
            return ([], ex.Problems);
        }
    }

    protected virtual (List<TFeature>, ZipArchiveProblems) ReadFeatures(
        FeatureType featureType,
        ZipArchiveEntry entry,
        IDbaseRecordEnumerator<TDbaseRecord> records,
        ZipArchiveFeatureReaderContext context)
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
                        var (feature, recordProblems) = ConvertToFeature(featureType, records.CurrentRecordNumber, record, context);

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
}
