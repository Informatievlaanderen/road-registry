namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class NumberedRoadChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<NumberedRoadChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> records)
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
                                problems += recordContext.FieldHasValueNull(record.RECORDTYPE.Field);
                            } else
                            {
                                if (!RecordType.ByIdentifier.ContainsKey(record.RECORDTYPE.Value.Value))
                                {
                                    problems += recordContext.RecordTypeMismatch(record.RECORDTYPE.Value.Value);
                                }
                            }
                            if (record.GW_OIDN.Value.HasValue)
                            {
                                if (record.GW_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new AttributeId(record.GW_OIDN.Value.Value);
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
                                problems += recordContext.IdentifierMissing();
                            }

                            if (record.IDENT8.Value == null)
                            {
                                problems += recordContext.FieldHasValueNull(record.IDENT8.Field);
                            }
                            else if (!NumberedRoadNumber.CanParse(record.IDENT8.Value))
                            {
                                problems += recordContext.NotNumberedRoadNumber(record.IDENT8.Value);
                            }

                            if (!record.RICHTING.Value.HasValue)
                            {
                                problems += recordContext.FieldHasValueNull(record.VOLGNUMMER.Field);
                            }
                            else if (!RoadSegmentNumberedRoadDirection.ByIdentifier.ContainsKey(record.RICHTING.Value.Value))
                            {
                                problems += recordContext.NumberedRoadDirectionMismatch(record.RICHTING.Value.Value);
                            }

                            if (!record.VOLGNUMMER.Value.HasValue)
                            {
                                problems += recordContext.FieldHasValueNull(record.VOLGNUMMER.Field);
                            }
                            else if (!RoadSegmentNumberedRoadOrdinal.Accepts(record.VOLGNUMMER.Value.Value))
                            {
                                problems += recordContext.NumberedRoadOrdinalOutOfRange(record.VOLGNUMMER.Value.Value);
                            }

                            if (!record.WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.FieldHasValueNull(record.WS_OIDN.Field);
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
                    problems += entry.HasNoDbaseRecords(true);
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
