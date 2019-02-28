namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordsValidator<TDbaseRecord>
        where TDbaseRecord : DbaseRecord, new()
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TDbaseRecord> records);
    }
}
