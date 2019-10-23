namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class NationalRoadChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<NationalRoadChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> records)
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
                            if (record.NW_OIDN.Value.HasValue)
                            {
                                if (record.NW_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new AttributeId(record.NW_OIDN.Value.Value);
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

                            if (record.IDENT2.Value == null)
                            {
                                problems += recordContext.FieldHasValueNull(record.IDENT2.Field);
                            }
                            else if (!NationalRoadNumber.CanParse(record.IDENT2.Value))
                            {
                                problems += recordContext.NotNationalRoadNumber(record.IDENT2.Value);
                            }

                            if (!record.WS_OIDN.Value.HasValue)
                            {
                                problems += recordContext.FieldHasValueNull(record.WS_OIDN.Field);
                            }
                            else if (!RoadSegmentId.Accepts(record.WS_OIDN.Value.Value))
                            {
                                problems += recordContext.RoadSegmentIdOutOfRange(record.WS_OIDN.Value.Value);
                            }

                            moved = records.MoveNext();
                        }
                    }
                }
                else
                {
                    problems += entry.HasNoDbaseRecords();
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
