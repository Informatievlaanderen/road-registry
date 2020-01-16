namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentSurfaceChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentSurfaceChangeDbaseRecord>
    {
        public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord> records)
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
                            if (record.WV_OIDN.Value.HasValue)
                            {
                                if (record.WV_OIDN.Value.Value == 0)
                                {
                                    problems += recordContext.IdentifierZero();
                                }
                                else
                                {
                                    var identifier = new AttributeId(record.WV_OIDN.Value.Value);
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
                                problems += recordContext.RequiredFieldIsNull(record.WV_OIDN.Field);
                            }

                            if (!record.TYPE.Value.HasValue)
                            {
                                problems += recordContext.RequiredFieldIsNull(record.TYPE.Field);
                            }
                            else if (!RoadSegmentSurfaceType.ByIdentifier.ContainsKey(record.TYPE.Value.Value))
                            {
                                problems += recordContext.SurfaceTypeMismatch(record.TYPE.Value.Value);
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
