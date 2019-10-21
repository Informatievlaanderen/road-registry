namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentWidthChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentWidthChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentWidthChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var fileContext = Problems.InFile(entry.Name);
            var problems = ZipArchiveProblems.None;

            try
            {
                var identifiers = new Dictionary<AttributeId, RecordNumber>();
                var moved = records.MoveNext();
                if (moved)
                {
                    while (moved)
                    {
                        var recordContext = fileContext.WithDbaseRecord(records.CurrentRecordNumber);
                        var record = records.Current;
                        if (record != null)
                        {
                            if (record.WB_OIDN.Value.HasValue)
                            {
                                if (record.WB_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new AttributeId(record.WB_OIDN.Value.Value);
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

                            if (!record.BREEDTE.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.BREEDTE);
                            }
                            else if (!RoadSegmentWidth.Accepts(record.BREEDTE.Value.Value))
                            {
                                problems += recordContext.WidthOutOfRange(record.BREEDTE.Value.Value);
                            }

                            if (!record.VANPOSITIE.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.VANPOSITIE);
                            }
                            else if (!RoadSegmentPosition.Accepts(record.VANPOSITIE.Value.Value))
                            {
                                problems += recordContext.FromPositionOutOfRange(record.VANPOSITIE.Value.Value);
                            }

                            if (!record.TOTPOSITIE.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.TOTPOSITIE);
                            }
                            else if (!RoadSegmentPosition.Accepts(record.TOTPOSITIE.Value.Value))
                            {
                                problems += recordContext.ToPositionOutOfRange(record.TOTPOSITIE.Value.Value);
                            }

                            if (!record.WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.FieldValueNull(record.WS_OIDN);
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
