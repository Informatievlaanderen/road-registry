namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveShapeEntryTranslator : IZipArchiveEntryTranslator
    {
        private readonly Encoding _encoding;
        private readonly IZipArchiveShapeRecordsTranslator _recordTranslator;

        public ZipArchiveShapeEntryTranslator(Encoding encoding, IZipArchiveShapeRecordsTranslator recordValidator)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _recordTranslator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, _encoding))
            {
                var header = ShapeFileHeader.Read(reader);
                var enumerator = header.CreateShapeRecordEnumerator(reader);
                return _recordTranslator.Translate(entry, enumerator, changes);
            }
        }
    }
}
