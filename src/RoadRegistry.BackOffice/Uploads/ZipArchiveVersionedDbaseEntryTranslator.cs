namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveVersionedDbaseEntryTranslator : IZipArchiveEntryTranslator
    {
        private readonly Encoding _encoding;
        private readonly DbaseFileHeaderReadBehavior _readBehavior;
        private readonly IReadOnlyDictionary<DbaseSchema, IZipArchiveEntryTranslator> _versionedTranslators;

        public ZipArchiveVersionedDbaseEntryTranslator(
            Encoding encoding,
            DbaseFileHeaderReadBehavior readBehavior,
            IReadOnlyDictionary<DbaseSchema, IZipArchiveEntryTranslator> versionedTranslators)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _readBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
            _versionedTranslators = versionedTranslators ?? throw new ArgumentNullException(nameof(versionedTranslators));
        }

        public TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, _encoding))
            {
                var header = DbaseFileHeader.Read(reader, _readBehavior);

                var translator = _versionedTranslators.Single(v => v.Key == header.Schema).Value;
                return translator.Translate(entry, changes);
            }
        }
    }
}
