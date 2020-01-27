namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class RoadSegmentLaneChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentLaneChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var problems = ZipArchiveProblems.None;

            try
            {
                var identifiers = new Dictionary<AttributeId, RecordNumber>();
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
                            if (record.RS_OIDN.Value.HasValue)
                            {
                                if (record.RS_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new AttributeId(record.RS_OIDN.Value.Value);
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
                                problems += recordContext.RequiredFieldIsNull(record.RS_OIDN.Field);
                            }

                            if (!record.AANTAL.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.AANTAL.Field);
                            }
                            else if (!RoadSegmentLaneCount.Accepts(record.AANTAL.Value.Value))
                            {
                                problems += recordContext.LaneCountOutOfRange(record.AANTAL.Value.Value);
                            }

                            if (!record.RICHTING.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.RICHTING.Field);
                            }
                            else if (!RoadSegmentLaneDirection.ByIdentifier.ContainsKey(record.RICHTING.Value.Value))
                            {
                                problems += recordContext.LaneDirectionMismatch(record.RICHTING.Value.Value);
                            }

                            if (!record.VANPOSITIE.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.VANPOSITIE.Field);
                            }
                            else if (!RoadSegmentPosition.Accepts(record.VANPOSITIE.Value.Value))
                            {
                                problems += recordContext.FromPositionOutOfRange(record.VANPOSITIE.Value.Value);
                            }

                            if (!record.TOTPOSITIE.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.TOTPOSITIE.Field);
                            }
                            else if (!RoadSegmentPosition.Accepts(record.TOTPOSITIE.Value.Value))
                            {
                                problems += recordContext.ToPositionOutOfRange(record.TOTPOSITIE.Value.Value);
                            }

                            if (!record.WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.WS_OIDN.Field);
                            }
                            else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value.Value))
                            {
                                problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value.Value);
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
