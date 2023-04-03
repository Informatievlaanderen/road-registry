namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;
using Uploads;

public class RoadSegmentSurfaceAttributeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentSurfaceAttributeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentSurfaceAttributeDbaseRecord> records, ZipArchiveValidationContext context)
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
                        if (record.WV_OIDN.HasValue)
                        {
                            if (record.WV_OIDN.Value == 0) problems += recordContext.IdentifierZero();
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WV_OIDN.Field);
                        }

                        if (!record.TYPE.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                        else if (!RoadSegmentSurfaceType.ByIdentifier.ContainsKey(record.TYPE.Value)) problems += recordContext.SurfaceTypeMismatch(record.TYPE.Value);

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
