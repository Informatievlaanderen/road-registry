namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema.RoadSegments;
using Uploads;

public class RoadSegmentDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentDbaseRecord> records, ZipArchiveValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(nameof(entry));
        ArgumentNullException.ThrowIfNull(nameof(records));
        ArgumentNullException.ThrowIfNull(nameof(context));

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
                        if (record.WS_OIDN.HasValue)
                        {
                            if (record.WS_OIDN.Value == 0)
                            {
                                problems += recordContext.IdentifierZero();
                            }
                            else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value))
                            {
                                problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value);
                            }
                        }
                        else
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                        }

                        if (!record.TGBEP.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.TGBEP.Field);
                        }
                        else if (!RoadSegmentAccessRestriction.ByIdentifier.ContainsKey(record.TGBEP.Value))
                        {
                            problems += recordContext.RoadSegmentAccessRestrictionMismatch(record.TGBEP.Value);
                        }

                        if (!record.STATUS.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.STATUS.Field);
                        }
                        else if (!RoadSegmentStatus.ByIdentifier.ContainsKey(record.STATUS.Value))
                        {
                            problems += recordContext.RoadSegmentStatusMismatch(record.STATUS.Value);
                        }

                        if (!record.WEGCAT.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.WEGCAT.Field);
                        }
                        else if (!RoadSegmentCategory.ByIdentifier.ContainsKey(record.WEGCAT.Value))
                        {
                            problems += recordContext.RoadSegmentCategoryMismatch(record.WEGCAT.Value);
                        }

                        if (!record.METHODE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.METHODE.Field);
                        }
                        else if (!RoadSegmentGeometryDrawMethod.ByIdentifier.ContainsKey(record.METHODE.Value))
                        {
                            problems += recordContext.RoadSegmentGeometryDrawMethodMismatch(record.METHODE.Value);
                        }

                        if (!record.MORF.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.MORF.Field);
                        }
                        else if (!RoadSegmentMorphology.ByIdentifier.ContainsKey(record.MORF.Value))
                        {
                            problems += recordContext.RoadSegmentMorphologyMismatch(record.MORF.Value);
                        }

                        if (!record.B_WK_OIDN.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.B_WK_OIDN.Field);
                        }
                        else if (!RoadNodeId.Accepts(record.B_WK_OIDN.Value))
                        {
                            problems += recordContext.BeginRoadNodeIdOutOfRange(record.B_WK_OIDN.Value);
                        }

                        if (!record.E_WK_OIDN.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.E_WK_OIDN.Field);
                        }
                        else if (!RoadNodeId.Accepts(record.E_WK_OIDN.Value))
                        {
                            problems += recordContext.EndRoadNodeIdOutOfRange(record.E_WK_OIDN.Value);
                        }

                        if (record.LSTRNMID.Value.HasValue && !CrabStreetnameId.Accepts(record.LSTRNMID.Value.Value))
                        {
                            problems += recordContext.LeftStreetNameIdOutOfRange(record.LSTRNMID.Value.Value);
                        }

                        if (record.RSTRNMID.Value.HasValue && !CrabStreetnameId.Accepts(record.RSTRNMID.Value.Value))
                        {
                            problems += recordContext.RightStreetNameIdOutOfRange(record.RSTRNMID.Value.Value);
                        }

                        if (record.B_WK_OIDN.HasValue && record.E_WK_OIDN.HasValue &&
                            record.B_WK_OIDN.Value.Equals(record.E_WK_OIDN.Value))
                        {
                            problems += recordContext.BeginRoadNodeIdEqualsEndRoadNode(
                                record.B_WK_OIDN.Value,
                                record.E_WK_OIDN.Value);
                        }

                        if (string.IsNullOrEmpty(record.BEHEER.Value))
                        {
                            problems += recordContext.RequiredFieldIsNull(record.BEHEER.Field);
                        }
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
