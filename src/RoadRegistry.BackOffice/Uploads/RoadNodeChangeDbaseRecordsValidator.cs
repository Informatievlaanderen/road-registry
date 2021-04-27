namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;

    public class RoadNodeChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadNodeChangeDbaseRecord>
    {
        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> records, ZipArchiveValidationContext context)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = ZipArchiveProblems.None;
            try
            {
                var identifiers = new Dictionary<RoadNodeId, RecordNumber>();
                var moved = records.MoveNext();
                if (moved)
                {
                    while (moved)
                    {
                        var recordContext = entry.AtDbaseRecord(records.CurrentRecordNumber);
                        var record = records.Current;
                        if (record != null)
                        {
                            if (!record.RECORDTYPE.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.RECORDTYPE.Field);
                            } else
                            {
                                if (!RecordType.ByIdentifier.ContainsKey(record.RECORDTYPE.Value))
                                {
                                    problems += recordContext.RecordTypeMismatch(record.RECORDTYPE.Value);
                                }
                            }
                            if (record.WEGKNOOPID.HasValue)
                            {
                                if (record.WEGKNOOPID.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new RoadNodeId(record.WEGKNOOPID.Value);
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
                                problems += recordContext.RequiredFieldIsNull(record.WEGKNOOPID.Field);
                            }

                            if (!record.TYPE.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                            }
                            else if (!RoadNodeType.ByIdentifier.ContainsKey(record.TYPE.Value))
                            {
                                problems += recordContext.RoadNodeTypeMismatch(record.TYPE.Value);
                            }

                            moved = records.MoveNext();
                        }
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
}
