namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;

public class ZipArchiveShapeEntryValidator : IZipArchiveEntryValidator
{
    private readonly Encoding _encoding;
    private readonly IZipArchiveShapeRecordsValidator _recordsValidator;
    private readonly IZipArchiveShapeRecordValidator _recordValidator;
    //TODO-rik obsolete? pas testen aan de andere
    public ZipArchiveShapeEntryValidator(Encoding encoding, IZipArchiveShapeRecordsValidator recordValidator)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _recordsValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
    }
    public ZipArchiveShapeEntryValidator(Encoding encoding, IZipArchiveShapeRecordValidator recordValidator)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
    }

    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;

        if (_recordValidator is not null)
        {
            var shpReader = new ZipArchiveShapeFileReader(_encoding);
            RecordNumber? previousRecordNumber = null;
            try
            {
                var hasRecord = false;
                foreach (var (geometry, recordNumber) in shpReader.Read(entry))
                {
                    hasRecord = true;

                    if (!geometry.IsValid)
                    {
                        var recordContext = entry.AtShapeRecord(recordNumber);
                        problems += recordContext.ShapeRecordGeometryMismatch();
                    }
                    else
                    {
                        var (recordProblems, recordContext) = _recordValidator.Validate(entry, recordNumber, geometry, context);
                        problems += recordProblems;
                        context = recordContext;
                    }

                    previousRecordNumber = recordNumber;
                }

                if (!hasRecord)
                {
                    problems += entry.HasNoShapeRecords();
                }
            }
            catch (Exception exception)
            {
                problems += entry.AtShapeRecord(previousRecordNumber?.Next() ?? RecordNumber.Initial).HasShapeRecordFormatError(exception);
            }
        }
        else if (_recordsValidator is not null)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, _encoding))
            {
                ShapeFileHeader header = null;
                try
                {
                    header = ShapeFileHeader.Read(reader);
                }
                catch (Exception exception)
                {
                    problems += entry.HasShapeHeaderFormatError(exception);
                }

                if (header != null)
                    using (var records = header.CreateShapeRecordEnumerator(reader))
                    {
                        var (recordProblems, recordContext) = _recordsValidator.Validate(entry, records, context);
                        problems += recordProblems;
                        context = recordContext;
                    }
            }
        }

        return (problems, context);
    }
}
