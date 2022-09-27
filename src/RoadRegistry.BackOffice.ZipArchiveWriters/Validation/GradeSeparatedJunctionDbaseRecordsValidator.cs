namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema.GradeSeparatedJunctions;
using Uploads;

public class GradeSeparatedJunctionDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<GradeSeparatedJunctionDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionDbaseRecord> records, ZipArchiveValidationContext context)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (records == null) throw new ArgumentNullException(nameof(records));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = ZipArchiveProblems.None;
        try
        {
            var identifiers = new Dictionary<GradeSeparatedJunctionId, RecordNumber>();
            var moved = records.MoveNext();
            if (moved)
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        if (record.OK_OIDN.HasValue)
                        {
                            if (record.OK_OIDN.Value == 0) problems += recordContext.IdentifierZero();
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.OK_OIDN.Field);
                        }

                        if (record.TYPE.HasValue)
                        {
                            if (!GradeSeparatedJunctionType.ByIdentifier.ContainsKey(record.TYPE.Value)) problems += recordContext.GradeSeparatedJunctionTypeMismatch(record.TYPE.Value);
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                        }

                        if (!record.BO_WS_OIDN.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.BO_WS_OIDN.Field);
                        else if (!RoadSegmentId.Accepts(record.BO_WS_OIDN.Value)) problems += recordContext.UpperRoadSegmentIdOutOfRange(record.BO_WS_OIDN.Value);

                        if (!record.ON_WS_OIDN.HasValue)
                            problems += recordContext.RequiredFieldIsNull(record.ON_WS_OIDN.Field);
                        else if (!RoadSegmentId.Accepts(record.ON_WS_OIDN.Value)) problems += recordContext.LowerRoadSegmentIdOutOfRange(record.ON_WS_OIDN.Value);

                        moved = records.MoveNext();
                    }
                }
            else
                problems += entry.HasNoDbaseRecords(true);
        }
        catch (Exception exception)
        {
            problems += entry.AtDbaseRecord(records.CurrentRecordNumber).HasDbaseRecordFormatError(exception);
        }

        return (problems, context);
    }
}
