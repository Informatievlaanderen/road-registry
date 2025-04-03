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
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf;
using Uploads;

public abstract class ZipArchiveShapeFeatureReader<TDbaseRecord, TFeature> : IZipArchiveFeatureReader<TFeature>
    where TDbaseRecord : DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly DbaseSchema _dbaseSchema;
    private readonly bool _treatHasNoDbaseRecordsAsError;

    protected ZipArchiveShapeFeatureReader(Encoding encoding, DbaseSchema dbaseSchema, bool treatHasNoDbaseRecordsAsError = false)
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

            return ([], problems);
        }

        var shpFileName = featureType.ToShapeFileName(fileName);
        var shpEntry = archive.FindEntry(shpFileName);
        if (shpEntry is null)
        {
            problems += problems.RequiredFileMissing(shpFileName);
        }

        using var entryStream = entry.Open();
        using var stream = entryStream.CopyToNewMemoryStream();

        //TODO-pr implement shp reading
        Shapefile.OpenRead()
        using var reader = new DbfReader(stream, _encoding);

        try
        {
            var schema = ReadSchema(reader);

            if (!schema.Equals(_dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(dbfFileName, _dbaseSchema, schema);
            }

            return ReadFeatures(featureType, fileName, entry, reader.CreateDbaseRecordEnumerator<TDbaseRecord>(), context);
        }
        catch (DbaseHeaderFormatException ex)
        {
            problems += entry.HasDbaseHeaderFormatError(ex.InnerException);
        }
        catch (DbaseSchemaMismatchException ex)
        {
            problems += entry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
        }

        problems += archive.ValidateProjectionFile(featureType, fileName, _encoding);

        return ([], problems);
    }

    protected virtual (List<TFeature>, ZipArchiveProblems) ReadFeatures(
        FeatureType featureType,
        ExtractFileName fileName,
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
