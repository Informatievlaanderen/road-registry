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

            var fileContext = Problems.InFile(entry.Name);
            var problems = ZipArchiveProblems.None;

            try
            {
                var identifiers = new Dictionary<GradeSeparatedJunctionId, RecordNumber>();
                var moved = records.MoveNext();
                if (moved)
                {
                    while (moved)
                    {
                        var recordContext = fileContext.WithDbaseRecord(records.CurrentRecordNumber);
                        var record = records.Current;
                        if (record != null)
                        {
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
                                problems += recordContext.IdentifierMissing();
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
                                problems += recordContext.FieldValueNull(record.TYPE);
                            }

                            if (!record.BO_WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.BO_WS_OIDN);
                            }
                            else if (!RoadSegmentId.Accepts(record.BO_WS_OIDN.Value.Value))
                            {
                                problems += recordContext.UpperRoadSegmentIdOutOfRange(record.BO_WS_OIDN.Value.Value);
                            }

                            if (!record.ON_WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.ON_WS_OIDN);
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
                    problems += fileContext.NoDbaseRecords();
                }
            }
            catch (Exception exception)
            {
                problems += fileContext.WithDbaseRecord(records.CurrentRecordNumber).DbaseRecordFormatError(exception);
            }

            return problems;
        }
    }
}
