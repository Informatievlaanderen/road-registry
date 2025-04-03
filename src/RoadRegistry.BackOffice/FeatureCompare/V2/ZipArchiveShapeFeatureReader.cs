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
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using ShapeFile;
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

    protected abstract (TFeature, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, TDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context);

    public (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var problems = ZipArchiveProblems.None;

        var dbfFileName = featureType.ToDbaseFileName(fileName);
        var shpFileName = featureType.ToShapeFileName(fileName);

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

            return ReadFeatures(featureType, fileName, dbfEntry, reader.CreateShapefileRecordEnumerator<TDbaseRecord>(), context);
        }
        catch (DbaseHeaderFormatException ex)
        {
            problems += dbfEntry.HasDbaseHeaderFormatError(ex.InnerException);
        }
        catch (DbaseSchemaMismatchException ex)
        {
            problems += dbfEntry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
        }

        problems += archive.ValidateProjectionFile(featureType, fileName, _encoding);

        return ([], problems);
    }

    protected virtual (List<TFeature>, ZipArchiveProblems) ReadFeatures(
        FeatureType featureType,
        ExtractFileName fileName,
        ZipArchiveEntry entry,
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
                    var (feature, recordProblems) = ConvertToFeature(featureType, fileName, records.CurrentRecordNumber, record.Item1, record.Item2, context);

                    problems += recordProblems;
                    features.Add(feature);

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
