namespace RoadRegistry.BackOffice.Uploads.V1.Validation;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;

public class RoadSegmentChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentChangeDbaseRecord>
{
    public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> records, ZipArchiveValidationContext context)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (records == null)
        {
            throw new ArgumentNullException(nameof(records));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var problems = ZipArchiveProblems.None;
        try
        {
            var identifiers = new Dictionary<RoadSegmentId, RecordNumber>();
            var moved = records.MoveNext();
            if (moved)
            {
                while (moved)
                {
                    var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                    var record = records.Current;
                    if (record != null)
                    {
                        RecordType recordType = default;
                        if (!record.RECORDTYPE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.RECORDTYPE.Field);
                        }
                        else
                        {
                            if (!RecordType.ByIdentifier.TryGetValue(record.RECORDTYPE.Value, out recordType))
                            {
                                problems += recordContext.RecordTypeMismatch(record.RECORDTYPE.Value);
                            }
                            else if (!recordType.IsAnyOf(RecordType.Identical, RecordType.Added, RecordType.Modified, RecordType.Removed))
                            {
                                problems += recordContext.RecordTypeNotSupported(record.RECORDTYPE.Value, RecordType.Identical.Translation.Identifier, RecordType.Added.Translation.Identifier, RecordType.Modified.Translation.Identifier, RecordType.Removed.Translation.Identifier);
                            }
                        }

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
                            else
                            {
                                if (recordType != RecordType.Added)
                                {
                                    var identifier = new RoadSegmentId(record.WS_OIDN.Value);
                                    if (identifiers.TryGetValue(identifier, out var takenByRecordNumber))
                                    {
                                        problems += recordContext.IdentifierNotUnique(
                                            identifier,
                                            takenByRecordNumber
                                        );
                                    }
                                    else
                                    {
                                        identifiers.Add(identifier, records.CurrentRecordNumber);
                                    }
                                }

                                if (recordType == RecordType.Identical)
                                {
                                    context = context.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                }
                                else if (recordType == RecordType.Added)
                                {
                                    if (recordType == RecordType.Added && record.EVENTIDN.HasValue && record.EVENTIDN.Value != 0)
                                    {
                                        context = context.WithAddedRoadSegment(new RoadSegmentId(record.EVENTIDN.Value));
                                    }
                                    else
                                    {
                                        context = context.WithAddedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                    }
                                }
                                else if (recordType == RecordType.Modified)
                                {
                                    context = context.WithModifiedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                }
                                else if (recordType == RecordType.Removed)
                                {
                                    context = context.WithRemovedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                                }
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

                        if (!record.CATEGORIE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.CATEGORIE.Field);
                        }
                        else if (!RoadSegmentCategory.ByIdentifier.ContainsKey(record.CATEGORIE.Value))
                        {
                            problems += recordContext.RoadSegmentCategoryMismatch(record.CATEGORIE.Value);
                        }

                        if (!record.METHODE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.METHODE.Field);
                        }
                        else if (!RoadSegmentGeometryDrawMethod.ByIdentifier.ContainsKey(record.METHODE.Value))
                        {
                            problems += recordContext.RoadSegmentGeometryDrawMethodMismatch(record.METHODE.Value);
                        }

                        if (!record.MORFOLOGIE.HasValue)
                        {
                            problems += recordContext.RequiredFieldIsNull(record.MORFOLOGIE.Field);
                        }
                        else if (!RoadSegmentMorphology.ByIdentifier.ContainsKey(record.MORFOLOGIE.Value))
                        {
                            problems += recordContext.RoadSegmentMorphologyMismatch(record.MORFOLOGIE.Value);
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

                        if (string.IsNullOrEmpty(record.BEHEERDER.Value))
                        {
                            problems += recordContext.RequiredFieldIsNull(record.BEHEERDER.Field);
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
