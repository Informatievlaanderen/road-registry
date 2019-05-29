namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

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
                        var record = records.Current;
                        if (record != null)
                        {
                            if (record.RS_OIDN.Value.HasValue)
                            {
                                if (record.RS_OIDN.Value.Value == 0)
                                {
                                    problems = problems.IdentifierZero(entry.Name, records.CurrentRecordNumber);
                                }
                                else
                                {
                                    var identifier = new AttributeId(record.RS_OIDN.Value.Value);
                                    if (identifiers.TryGetValue(identifier, out var takenByRecordNumber))
                                    {
                                        problems = problems.IdentifierNotUnique(
                                            entry.Name,
                                            identifier,
                                            records.CurrentRecordNumber,
                                            takenByRecordNumber);
                                    }
                                    else
                                    {
                                        identifiers.Add(identifier, records.CurrentRecordNumber);
                                    }
                                }
                            }
                            else
                            {
                                problems = problems.IdentifierMissing(entry.Name, records.CurrentRecordNumber);
                            }

                            moved = records.MoveNext();
                        }
                    }
                }
                else
                {
                    problems = problems.NoDbaseRecords(entry.Name);
                }

            }
            catch (Exception exception)
            {
                problems = problems.DbaseRecordFormatError(entry.Name, records.CurrentRecordNumber, exception);
            }

            return problems;
        }
    }
}
