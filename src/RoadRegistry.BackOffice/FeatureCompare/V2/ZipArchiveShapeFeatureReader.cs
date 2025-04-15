namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using Extensions;
using Extracts;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using ShapeFile;
using Uploads;

public abstract class ZipArchiveShapeFeatureReader<TDbaseRecord, TFeature> : IZipArchiveFeatureReader<TFeature>
    where TDbaseRecord : DbaseRecord, new()
{
    protected readonly ExtractFileName FileName;
    private readonly Encoding _encoding;
    private readonly DbaseSchema _dbaseSchema;
    private readonly bool _treatHasNoDbaseRecordsAsError;

    protected ZipArchiveShapeFeatureReader(Encoding encoding, ExtractFileName fileName, DbaseSchema dbaseSchema, bool treatHasNoDbaseRecordsAsError = false)
    {
        FileName = fileName;
        _encoding = encoding;
        _dbaseSchema = dbaseSchema;
        _treatHasNoDbaseRecordsAsError = treatHasNoDbaseRecordsAsError;
    }

    protected abstract (TFeature, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, TDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context);

    public (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var problems = ZipArchiveProblems.None;

        var dbfFileName = featureType.ToDbaseFileName(FileName);
        var shpFileName = featureType.ToShapeFileName(FileName);

        var dbfEntry = archive.FindEntry(dbfFileName);
        var shpEntry = archive.FindEntry(shpFileName);

        if (dbfEntry is null || shpEntry is null)
        {
            if (dbfEntry is null)
            {
                problems += problems.RequiredFileMissing(dbfFileName);
            }

            if (shpEntry is null)
            {
                problems += problems.RequiredFileMissing(shpFileName);
            }

            return ([], problems);
        }

        using var dbfEntryStream = dbfEntry.Open();
        using var dbfStream = dbfEntryStream.CopyToNewMemoryStream();

        using var shpEntryStream = shpEntry.Open();
        using var shpStream = shpEntryStream.CopyToNewMemoryStream();

        using var reader = Shapefile.OpenRead(shpStream, dbfStream, new ShapefileReaderOptions
        {
            Encoding = _encoding,
            Factory = WellKnownGeometryFactories.WithoutMAndZ
        });

        try
        {
            var schema = ReadSchema(reader);

            if (!schema.Equals(_dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(dbfFileName, _dbaseSchema, schema);
            }

            return ReadFeatures(featureType, dbfEntry, shpEntry, reader.CreateShapefileRecordEnumerator<TDbaseRecord>(), context);
        }
        catch (DbaseHeaderFormatException ex)
        {
            problems += dbfEntry.HasDbaseHeaderFormatError(ex.InnerException);
        }
        catch (DbaseSchemaMismatchException ex)
        {
            problems += dbfEntry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
        }

        return ([], problems);
    }

    private (List<TFeature>, ZipArchiveProblems) ReadFeatures(
        FeatureType featureType,
        ZipArchiveEntry dbfEntry,
        ZipArchiveEntry shpEntry,
        IShapefileRecordEnumerator<TDbaseRecord> records,
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
                    var (feature, recordProblems) = ConvertToFeature(featureType, records.CurrentRecordNumber, record.Item1, record.Item2, context);

                    problems += recordProblems;
                    features.Add(feature);

                    moved = records.MoveNext();
                }
            }
            else
            {
                problems += dbfEntry.HasNoDbaseRecords(_treatHasNoDbaseRecordsAsError);
                problems += shpEntry.HasNoShapeRecords();
            }
        }
        catch (Exception exception)
        {
            problems += dbfEntry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (features, problems);
    }

    private DbaseSchema ReadSchema(ShapefileReader reader)
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
