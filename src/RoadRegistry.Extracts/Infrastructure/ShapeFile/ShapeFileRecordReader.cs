namespace RoadRegistry.Extracts.Infrastructure.ShapeFile;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using Uploads;

public class ShapeFileRecordReader
{
    private readonly Encoding _encoding;

    public ShapeFileRecordReader(Encoding encoding)
    {
        _encoding = encoding;
    }

    public ShapeFileReadResult<TDbaseRecord> ReadFromArchive<TDbaseRecord>(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        DbaseSchema dbaseSchema,
        GeometryFactory geometryFactory)
        where TDbaseRecord : DbaseRecord, new()
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

            throw new ZipArchiveValidationException(problems);
        }

        using var dbfEntryStream = dbfEntry.Open();
        using var shpEntryStream = shpEntry.Open();

        var dbfStream = dbfEntryStream.CopyToNewMemoryStream();
        var shpStream = shpEntryStream.CopyToNewMemoryStream();

        var reader = Shapefile.OpenRead(shpStream, dbfStream, new ShapefileReaderOptions
        {
            Encoding = _encoding,
            Factory = geometryFactory
        });

        try
        {
            var schema = ReadSchema(reader);

            if (!schema.Equals(dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(dbfFileName, dbaseSchema, schema);
            }

            return new ShapeFileReadResult<TDbaseRecord>(dbfStream, reader, dbfEntry, shpEntry);
        }
        catch (DbaseHeaderFormatException ex)
        {
            problems += dbfEntry.HasDbaseHeaderFormatError(ex.InnerException);
        }
        catch (DbaseSchemaMismatchException ex)
        {
            problems += dbfEntry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
        }
        catch (Exception)
        {
            dbfStream.Dispose();
            shpStream.Dispose();
            reader.Dispose();
            throw;
        }

        dbfStream.Dispose();
        shpStream.Dispose();
        reader.Dispose();

        throw new ZipArchiveValidationException(problems);
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

public sealed class ShapeFileReadResult<TDbaseRecord> : IDisposable
    where TDbaseRecord : DbaseRecord, new()
{
    public IShapeFileRecordEnumerator<TDbaseRecord> RecordEnumerator { get; }
    public ZipArchiveEntry DbaseEntry { get; }
    public ZipArchiveEntry ShapeEntry { get; }

    private readonly Stream _dbfStream;
    private readonly ShapefileReader _shpReader;

    public ShapeFileReadResult(Stream dbfStream, ShapefileReader shpReader, ZipArchiveEntry dbaseEntry, ZipArchiveEntry shapeEntry)
    {
        _dbfStream = dbfStream;
        _shpReader = shpReader;

        RecordEnumerator = shpReader.CreateShapefileRecordEnumerator<TDbaseRecord>();
        DbaseEntry = dbaseEntry;
        ShapeEntry = shapeEntry;
    }

    public void Dispose()
    {
        _dbfStream?.Dispose();
        _shpReader?.Dispose();
        RecordEnumerator?.Dispose();
    }
}
