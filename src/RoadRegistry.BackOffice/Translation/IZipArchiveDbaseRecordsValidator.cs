namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordsValidator<in TRecord>
        where TRecord : DbaseRecord, new()
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<TRecord> records);
    }
}
