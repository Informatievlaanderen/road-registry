namespace RoadRegistry.BackOffice.Dbase.V2;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Exceptions;
using Extensions;
using Extracts;
using NetTopologySuite.IO.Esri.Dbf;
using Uploads;

public class DbaseRecordReader
{
    private readonly Encoding _encoding;

    public DbaseRecordReader(Encoding encoding)
    {
        _encoding = encoding;
    }

    public DbaseReadResult<TDbaseRecord> ReadFromArchive<TDbaseRecord>(
        ZipArchive archive,
        ExtractFileName fileName,
        FeatureType featureType,
        DbaseSchema dbaseSchema)
        where TDbaseRecord : DbaseRecord, new()
    {
        var problems = ZipArchiveProblems.None;

        var dbfFileName = featureType.ToDbaseFileName(fileName);
        var entry = archive.FindEntry(dbfFileName);
        if (entry is null)
        {
            problems += problems.RequiredFileMissing(dbfFileName);

            throw new ZipArchiveValidationException(problems);
        }

        using var entryStream = entry.Open();

        var stream = entryStream.CopyToNewMemoryStream();
        var reader = new DbfReader(stream, _encoding);

        try
        {
            var schema = ReadSchema(reader);
            if (!schema.Equals(dbaseSchema))
            {
                throw new DbaseSchemaMismatchException(dbfFileName, dbaseSchema, schema);
            }

            return new DbaseReadResult<TDbaseRecord>(stream, reader, entry);
        }
        catch (DbaseHeaderFormatException ex)
        {
            problems += entry.HasDbaseHeaderFormatError(ex.InnerException);
        }
        catch (DbaseSchemaMismatchException ex)
        {
            problems += entry.HasDbaseSchemaMismatch(ex.ExpectedSchema, ex.ActualSchema);
        }
        catch (Exception)
        {
            stream.Dispose();
            reader.Dispose();
            throw;
        }

        stream.Dispose();
        reader.Dispose();

        throw new ZipArchiveValidationException(problems);
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

public sealed class DbaseReadResult<TDbaseRecord> : IDisposable
    where TDbaseRecord : DbaseRecord, new()
{
    public IDbaseRecordEnumerator<TDbaseRecord> RecordEnumerator { get; }
    public ZipArchiveEntry DbaseEntry { get; }

    private readonly Stream _dbfStream;
    private readonly DbfReader _dbfReader;

    public DbaseReadResult(Stream dbfStream, DbfReader dbfReader, ZipArchiveEntry dbaseEntry)
    {
        _dbfStream = dbfStream;
        _dbfReader = dbfReader;

        RecordEnumerator = dbfReader.CreateDbaseRecordEnumerator<TDbaseRecord>();
        DbaseEntry = dbaseEntry;
    }

    public void Dispose()
    {
        _dbfStream?.Dispose();
        _dbfReader?.Dispose();
        RecordEnumerator?.Dispose();
    }
}
