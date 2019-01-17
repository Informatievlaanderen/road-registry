namespace RoadRegistry.Translation
{
    using System;
    using System.IO.Compression;
    using Aiv.Vbr.Shaperon;

    public class EuropeanRoadDbaseRecordValidator : IZipArchiveDbaseRecordValidator
    {
        public ZipArchiveErrors Validate(ZipArchiveEntry entry, DbaseRecord record)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var errors = ZipArchiveErrors.None;

            // EU_OIDN must be unique / must not be 0

            var other = new EuropeanRoadComparisonDbaseRecord();
            other.PopulateFrom(record);

            return errors;
        }
    }
}