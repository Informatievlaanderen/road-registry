namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.RoadSegments;
using Uploads;

public class RoadSegmentWidthAttributeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentWidthAttributeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentWidthAttributeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        try
        {
            var moved = records.MoveNext();
            if (moved)
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        if (record.WB_OIDN.HasValue)
                        {
                            if (record.WB_OIDN.Value == 0) problems += recordContext.IdentifierZero();
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WB_OIDN.Field);
                        }

                        if (!record.BREEDTE.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.BREEDTE.Field);
                        else if (!RoadSegmentWidth.Accepts(record.BREEDTE.Value)) problems += recordContext.WidthOutOfRange(record.BREEDTE.Value);

                        if (!record.VANPOS.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.VANPOS.Field);
                        else if (!RoadSegmentPosition.Accepts(record.VANPOS.Value)) problems += recordContext.FromPositionOutOfRange(record.VANPOS.Value);

                        if (!record.TOTPOS.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.TOTPOS.Field);
                        else if (!RoadSegmentPosition.Accepts(record.TOTPOS.Value)) problems += recordContext.ToPositionOutOfRange(record.TOTPOS.Value);

                        if (record.VANPOS.HasValue && record.TOTPOS.HasValue &&
                            record.VANPOS.Value >= record.TOTPOS.Value)
                            problems += recordContext.FromPositionEqualToOrGreaterThanToPosition(
                                record.VANPOS.Value,
                                record.TOTPOS.Value);

                        if (!record.WS_OIDN.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                        else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value))
                            problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value);
                    }

                    moved = records.MoveNext();
                }
            else
                problems += entry.HasNoDbaseRecords(false);
        }
        catch (Exception exception)
        {
            problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (problems, context);
    }
}
