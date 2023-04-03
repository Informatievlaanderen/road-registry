namespace RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;
using Schema;

public class RoadSegmentEuropeanRoadAttributeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentEuropeanRoadAttributeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentEuropeanRoadAttributeDbaseRecord> records, ZipArchiveValidationContext context)
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
                        if (record.EU_OIDN.HasValue)
                        {
                            if (record.EU_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.EU_OIDN.Field);
                        }

                        if (!record.EUNUMMER.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.EUNUMMER.Field);
                        }
                        else if (!EuropeanRoadNumber.CanParse(record.EUNUMMER.Value))
                        {
                            problems += recordContext.NotEuropeanRoadNumber(record.EUNUMMER.Value);
                        }

                        if (!record.WS_OIDN.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                        }
                        else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value))
                        {
                            problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value);
                        }
                    }

                    moved = records.MoveNext();
                }
            }
            else
            {
                problems += entry.HasNoDbaseRecords(true);
            }
        }
        catch (Exception exception)
        {
            problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (problems, context);
    }
}
