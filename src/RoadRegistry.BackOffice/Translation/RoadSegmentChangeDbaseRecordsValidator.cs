namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

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
                            if (record.RECORDTYPE.Value == null)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.RECORDTYPE.Field);
                            } else
                            {
                                if (!RecordType.ByIdentifier.ContainsKey(record.RECORDTYPE.Value.Value))
                                {
                                    problems += recordContext.RecordTypeMismatch(record.RECORDTYPE.Value.Value);
                                }
                            }
                            if (record.WS_OIDN.Value.HasValue)
                            {
                                if (record.WS_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new RoadSegmentId(record.WS_OIDN.Value.Value);
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
                            }
                            else
                            {
                                problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                            }

                            if (!record.TGBEP.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.TGBEP.Field);
                            }
                            else if (!RoadSegmentAccessRestriction.ByIdentifier.ContainsKey(record.TGBEP.Value.Value))
                            {
                                problems += recordContext.RoadSegmentAccessRestrictionMismatch(record.TGBEP.Value.Value);
                            }

                            if (!record.STATUS.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.STATUS.Field);
                            }
                            else if (!RoadSegmentStatus.ByIdentifier.ContainsKey(record.STATUS.Value.Value))
                            {
                                problems += recordContext.RoadSegmentStatusMismatch(record.STATUS.Value.Value);
                            }

                            if (record.WEGCAT.Value == null)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.WEGCAT.Field);
                            }
                            else if (!RoadSegmentCategory.ByIdentifier.ContainsKey(record.WEGCAT.Value))
                            {
                                problems += recordContext.RoadSegmentCategoryMismatch(record.WEGCAT.Value);
                            }

                            if (!record.METHODE.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.METHODE.Field);
                            }
                            else if (!RoadSegmentGeometryDrawMethod.ByIdentifier.ContainsKey(record.METHODE.Value.Value))
                            {
                                problems += recordContext.RoadSegmentGeometryDrawMethodMismatch(record.METHODE.Value.Value);
                            }

                            if (!record.MORFOLOGIE.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.MORFOLOGIE.Field);
                            }
                            else if (!RoadSegmentMorphology.ByIdentifier.ContainsKey(record.MORFOLOGIE.Value.Value))
                            {
                                problems += recordContext.RoadSegmentMorphologyMismatch(record.MORFOLOGIE.Value.Value);
                            }

                            if (!record.B_WK_OIDN.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.B_WK_OIDN.Field);
                            }
                            else if (!RoadNodeId.Accepts(record.B_WK_OIDN.Value.Value))
                            {
                                problems += recordContext.BeginRoadNodeIdOutOfRange(record.B_WK_OIDN.Value.Value);
                            }

                            if (!record.E_WK_OIDN.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.E_WK_OIDN.Field);
                            }
                            else if (!RoadNodeId.Accepts(record.E_WK_OIDN.Value.Value))
                            {
                                problems += recordContext.EndRoadNodeIdOutOfRange(record.E_WK_OIDN.Value.Value);
                            }

                            if (record.BEHEERDER.Value == null)
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

            return problems;
        }
    }
}
