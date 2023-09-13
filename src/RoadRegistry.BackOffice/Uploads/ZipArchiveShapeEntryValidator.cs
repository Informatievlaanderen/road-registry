namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using System;
using System.IO.Compression;
using ShapeFile;

public class ZipArchiveShapeEntryValidator : IZipArchiveEntryValidator
{
    private readonly IZipArchiveShapeRecordValidator _recordValidator;

    public ZipArchiveShapeEntryValidator(IZipArchiveShapeRecordValidator recordValidator)
    {
        _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
    }

    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;

        var shpReader = new ZipArchiveShapeFileReader();
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

        return (problems, context);
    }
}
