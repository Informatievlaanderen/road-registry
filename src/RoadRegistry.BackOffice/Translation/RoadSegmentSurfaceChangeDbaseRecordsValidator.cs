namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class RoadSegmentSurfaceChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<RoadSegmentSurfaceChangeDbaseRecord>
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<RoadSegmentSurfaceChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var errors = ZipArchiveErrors.None;
            var recordNumber = RecordNumber.Initial;
            try
            {
                var identifiers = new Dictionary<AttributeId, RecordNumber>();
                var count = 0;
                while (records.MoveNext())
                {
                    var record = records.Current;
                    if (record != null)
                    {
                        if (record.WV_OIDN.Value.HasValue)
                        {
                            if (record.WV_OIDN.Value.Value == 0)
                            {
                                errors = errors.IdentifierZero(entry.Name, recordNumber);
                            }
                            else
                            {
                                var identifier = new AttributeId(record.WV_OIDN.Value.Value);
                                if (identifiers.TryGetValue(identifier, out var takenByRecordNumber))
                                {
                                    errors = errors.IdentifierNotUnique(entry.Name, identifier, recordNumber,
                                        takenByRecordNumber);
                                }
                                else
                                {
                                    identifiers.Add(identifier, recordNumber);
                                }
                            }
                        }
                        else
                        {
                            errors = errors.IdentifierMissing(entry.Name, recordNumber);
                        }

                        count++;
                        recordNumber = recordNumber.Next();
                    }
                }

                if (count == 0)
                {
                    errors = errors.NoDbaseRecords(entry.Name);
                }
            }
            catch (Exception exception)
            {
                errors = errors.DbaseRecordFormatError(entry.Name, recordNumber, exception);
            }

            return errors;
        }
    }
}
