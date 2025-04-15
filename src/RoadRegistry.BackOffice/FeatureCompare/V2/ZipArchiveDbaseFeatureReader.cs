namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using Extensions;
using Extracts;
using NetTopologySuite.IO.Esri.Dbf;
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
        var problems = ZipArchiveProblems.None;

        var dbfFileName = featureType.ToDbaseFileName(FileName);
        var entry = archive.FindEntry(dbfFileName);
        if (entry is null)
        {
            problems += problems.RequiredFileMissing(dbfFileName);

            return ([], problems);
        }

        using var entryStream = entry.Open();
        using var stream = entryStream.CopyToNewMemoryStream();
        using var reader = new DbfReader(stream, _encoding);

        try
        {
            var schema = ReadSchema(reader);
            if (!schema.Equals(_dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(dbfFileName, _dbaseSchema, schema);
            }

            return ReadFeatures(featureType, entry, reader.CreateDbaseRecordEnumerator<TDbaseRecord>(), context);
        }
        catch (DbaseHeaderFormatException ex)
        {
            problems += entry.HasDbaseHeaderFormatError(ex.InnerException);
        }
        catch (DbaseSchemaMismatchException ex)
        {
            problems += entry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
        }

        return ([], problems);
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

    private DbaseSchema ReadSchema(DbfReader reader)
    {
        try
        {
            return reader.Fields.ToDbaseSchema();
        }
        catch (Exception exception)
        {
            throw new DbaseHeaderFormatException(exception);
        }
    }
}
