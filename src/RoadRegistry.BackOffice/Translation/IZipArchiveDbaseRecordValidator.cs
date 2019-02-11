namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, DbaseRecord record);
    }

    public interface IZipArchiveDbaseRecordValidator<in TRecord>
        where TRecord : DbaseRecord, new()
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, TRecord record);
    }

    public interface IZipArchiveDbaseRecordsValidator<in TRecord>
        where TRecord : DbaseRecord, new()
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerable<TRecord> records);
    }
}
