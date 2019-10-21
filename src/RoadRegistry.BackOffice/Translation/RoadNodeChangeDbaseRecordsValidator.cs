namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadNodeChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadNodeChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var fileContext = Problems.InFile(entry.Name);
            var problems = ZipArchiveProblems.None;

            try
            {
                var identifiers = new Dictionary<RoadNodeId, RecordNumber>();
                var moved = records.MoveNext();
                if (moved)
                {
                    while (moved)
                    {
                        var recordContext = fileContext.WithDbaseRecord(records.CurrentRecordNumber);
                        var record = records.Current;
                        if (record != null)
                        {
                            if (record.WEGKNOOPID.Value.HasValue)
                            {
                                if (record.WEGKNOOPID.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new RoadNodeId(record.WEGKNOOPID.Value.Value);
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

                            if (!record.TYPE.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.TYPE);
                            }
                            else if (!RoadNodeType.ByIdentifier.ContainsKey(record.TYPE.Value.Value))
                            {
                                problems += recordContext.RoadNodeTypeMismatch(record.TYPE.Value.Value);
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
