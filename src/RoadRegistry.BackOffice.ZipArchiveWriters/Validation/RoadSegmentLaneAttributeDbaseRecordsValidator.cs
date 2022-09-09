namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema.RoadSegments;
using Uploads;

public class RoadSegmentLaneAttributeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentLaneAttributeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentLaneAttributeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (records == null) throw new ArgumentNullException(nameof(records));
        if (context == null) throw new ArgumentNullException(nameof(context));

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
                        if (record.RS_OIDN.HasValue)
                        {
                            if (record.RS_OIDN.Value == 0) problems += recordContext.IdentifierZero();
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.RS_OIDN.Field);
                        }

                        if (!record.AANTAL.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.AANTAL.Field);
                        else if (!RoadSegmentLaneCount.Accepts(record.AANTAL.Value)) problems += recordContext.LaneCountOutOfRange(record.AANTAL.Value);

                        if (!record.RICHTING.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.RICHTING.Field);
                        else if (!RoadSegmentLaneDirection.ByIdentifier.ContainsKey(record.RICHTING.Value)) problems += recordContext.LaneDirectionMismatch(record.RICHTING.Value);

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
