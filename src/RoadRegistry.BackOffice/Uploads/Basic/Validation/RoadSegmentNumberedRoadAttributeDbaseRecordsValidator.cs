namespace RoadRegistry.BackOffice.Uploads.Basic.Validation;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;

public class RoadSegmentNumberedRoadAttributeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentNumberedRoadAttributeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentNumberedRoadAttributeDbaseRecord> records, ZipArchiveValidationContext context)
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
                        if (record.GW_OIDN.HasValue)
                        {
                            if (record.GW_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.GW_OIDN.Field);
                        }

                        if (!record.IDENT8.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.IDENT8.Field);
                        }
                        else if (!NumberedRoadNumber.CanParse(record.IDENT8.Value))
                        {
                            problems += recordContext.NotNumberedRoadNumber(record.IDENT8.Value);
                        }

                        if (!record.RICHTING.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.RICHTING.Field);
                        }
                        else if (!RoadSegmentNumberedRoadDirection.ByIdentifier.ContainsKey(record.RICHTING.Value))
                        {
                            problems += recordContext.NumberedRoadDirectionMismatch(record.RICHTING.Value);
                        }

                        if (!record.VOLGNUMMER.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.VOLGNUMMER.Field);
                        }
                        else if (!RoadSegmentNumberedRoadOrdinal.Accepts(record.VOLGNUMMER.Value))
                        {
                            problems += recordContext.NumberedRoadOrdinalOutOfRange(record.VOLGNUMMER.Value);
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
