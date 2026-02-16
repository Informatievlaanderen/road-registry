namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Uploads;

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
        try
        {
            using var shape = new ShapeFileRecordReader(_encoding)
                .ReadFromArchive<TDbaseRecord>(archive, FileName, featureType, _dbaseSchema, WellKnownGeometryFactories.Lambert08WithoutMAndZ);

            return ReadFeatures(featureType, shape.DbaseEntry, shape.ShapeEntry, shape.RecordEnumerator, context);
        }
        catch (ZipArchiveValidationException ex)
        {
            return ([], ex.Problems);
        }
    }

    private (List<TFeature>, ZipArchiveProblems) ReadFeatures(
        FeatureType featureType,
        ZipArchiveEntry dbfEntry,
        ZipArchiveEntry shpEntry,
        IShapeFileRecordEnumerator<TDbaseRecord> records,
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
}
