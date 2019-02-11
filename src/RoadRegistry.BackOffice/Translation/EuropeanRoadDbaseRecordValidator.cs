namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class EuropeanRoadDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<EuropeanRoadChangeDbaseRecord>
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, DbaseRecord record)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var errors = ZipArchiveErrors.None;

            // EU_OIDN must be unique / must not be 0

            var other = new EuropeanRoadChangeDbaseRecord();
            //other.PopulateFrom(record);

            return errors;
        }

        public ZipArchiveErrors Validate(ZipArchiveEntry entry, EuropeanRoadChangeDbaseRecord record)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var errors = ZipArchiveErrors.None;

            // EU_OIDN must be unique / must not be 0

            var other = new EuropeanRoadChangeDbaseRecord();
            //other.PopulateFrom(record);

            return errors;
        }

        public ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerable<EuropeanRoadChangeDbaseRecord> records)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var errors = ZipArchiveErrors.None;

            // EU_OIDN must be unique / must not be 0

            return errors;
        }
    }
}
