namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class GradeSeparatedJunctionChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<GradeSeparatedJunctionChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var problems = ZipArchiveProblems.None;

            try
            {
                var identifiers = new Dictionary<GradeSeparatedJunctionId, RecordNumber>();
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
                            if (record.OK_OIDN.Value.HasValue)
                            {
                                if (record.OK_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new GradeSeparatedJunctionId(record.OK_OIDN.Value.Value);
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
                                problems += recordContext.RequiredFieldIsNull(record.OK_OIDN.Field);
                            }

                            if (record.TYPE.Value.HasValue)
                            {
                                if (!GradeSeparatedJunctionType.ByIdentifier.ContainsKey(record.TYPE.Value.Value))
                                {
                                    problems += recordContext.GradeSeparatedJunctionTypeMismatch(record.TYPE.Value.Value);
                                }
                            }
                            else
                            {
                                problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                            }

                            if (!record.BO_WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.BO_WS_OIDN.Field);
                            }
                            else if (!RoadSegmentId.Accepts(record.BO_WS_OIDN.Value.Value))
                            {
                                problems += recordContext.UpperRoadSegmentIdOutOfRange(record.BO_WS_OIDN.Value.Value);
                            }

                            if (!record.ON_WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.ON_WS_OIDN.Field);
                            }
                            else if (!RoadSegmentId.Accepts(record.ON_WS_OIDN.Value.Value))
                            {
                                problems += recordContext.LowerRoadSegmentIdOutOfRange(record.ON_WS_OIDN.Value.Value);
                            }

                            moved = records.MoveNext();
                        }
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
