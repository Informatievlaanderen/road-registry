namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Messages;
    using Model;

    public class GradeSeparatedJunctionChangeDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<GradeSeparatedJunctionChangeDbaseRecord>
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var errors = ZipArchiveErrors.None;
            try
            {
                var identifiers = new Dictionary<GradeSeparatedJunctionId, RecordNumber>();
                var moved = records.MoveNext();
                if (moved)
                {
                    while (moved)
                    {
                        var record = records.Current;
                        if (record != null)
                        {
                            if (record.OK_OIDN.Value.HasValue)
                            {
                                if (record.OK_OIDN.Value.Value == 0)
                                {
                                    errors = errors.IdentifierZero(entry.Name, records.CurrentRecordNumber);
                                }
                                else
                                {
                                    var identifier = new GradeSeparatedJunctionId(record.OK_OIDN.Value.Value);
                                    if (identifiers.TryGetValue(identifier, out var takenByRecordNumber))
                                    {
                                        errors = errors.IdentifierNotUnique(
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
                                errors = errors.IdentifierMissing(entry.Name, records.CurrentRecordNumber);
                            }

                            moved = records.MoveNext();
                        }
                    }
                }
                else
                {
                    errors = errors.NoDbaseRecords(entry.Name);
                }
            }
            catch (Exception exception)
            {
                errors = errors.DbaseRecordFormatError(entry.Name, records.CurrentRecordNumber, exception);
            }

            return errors;
        }
    }
}
