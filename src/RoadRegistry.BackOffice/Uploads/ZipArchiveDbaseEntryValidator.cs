namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class ZipArchiveDbaseEntryValidator<TDbaseRecord> : IZipArchiveEntryValidator
    where TDbaseRecord : DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly DbaseFileHeaderReadBehavior _readBehavior;
    private readonly IZipArchiveDbaseRecordsValidator<TDbaseRecord> _recordValidator;
    private readonly DbaseSchema _schema;

    public ZipArchiveDbaseEntryValidator(
        Encoding encoding,
        DbaseFileHeaderReadBehavior readBehavior,
        DbaseSchema schema,
        IZipArchiveDbaseRecordsValidator<TDbaseRecord> recordValidator)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _readBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
    }

    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = ZipArchiveProblems.None;
        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, _encoding))
        {
            DbaseFileHeader header = null;
            try
            {
                header = DbaseFileHeader.Read(reader, _readBehavior);
            }
            catch (Exception exception)
            {
                problems += entry.HasDbaseHeaderFormatError(exception);
            }

            if (header != null)
            {
                if (!header.Schema.Equals(_schema))
                    problems += entry.HasDbaseSchemaMismatch(_schema, header.Schema);
                else
                    using (var records = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader))
                    {
                        var (recordProblems, recordContext) = _recordValidator.Validate(entry, records, context);
                        problems += recordProblems;
                        context = recordContext;
                    }
            }
        }

        return (problems, context);
    }
}
