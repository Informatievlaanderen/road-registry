namespace RoadRegistry.BackOffice.Dbase.UploadsBeforeFeatureCompare.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Uploads;

public class RoadNodeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadNodeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        if (record.WK_OIDN.HasValue)
                        {
                            if (record.WK_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WK_OIDN.Field);
                        }

                        if (!record.TYPE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                        }
                        else if (!RoadNodeType.ByIdentifier.ContainsKey(record.TYPE.Value))
                        {
                            problems += recordContext.RoadNodeTypeMismatch(record.TYPE.Value);
                        }

                        moved = records.MoveNext();
                    }
                }
            }
            else
            {
                problems += entry.HasNoDbaseRecords(false);
            }
        }
        catch (Exception exception)
        {
            problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (problems, context);
    }
}
