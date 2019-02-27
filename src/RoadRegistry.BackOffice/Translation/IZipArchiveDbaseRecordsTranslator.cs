namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordsTranslator<TRecord> where TRecord : DbaseRecord
    {
        TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<TRecord> records, TranslatedChanges changes);
    }
}