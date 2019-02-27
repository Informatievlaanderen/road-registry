namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveDbaseEntryTranslator<TRecord> : IZipArchiveEntryTranslator
        where TRecord : DbaseRecord
    {
        private readonly IZipArchiveDbaseRecordsTranslator<TRecord> _translator;

        public ZipArchiveDbaseEntryTranslator(IZipArchiveDbaseRecordsTranslator<TRecord> translator)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (changes == null) throw new ArgumentNullException(nameof(changes));
            // todo: read header and pass on to translator with record enumerator
            return changes;
        }
    }
}