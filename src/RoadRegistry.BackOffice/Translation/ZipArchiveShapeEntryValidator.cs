namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveShapeEntryValidator : IZipArchiveEntryValidator
    {
        private readonly Encoding _encoding;
        private readonly IZipArchiveShapeRecordsValidator _recordValidator;

        public ZipArchiveShapeEntryValidator(Encoding encoding, IZipArchiveShapeRecordsValidator recordValidator)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public ZipArchiveErrors Validate(ZipArchiveEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var errors = ZipArchiveErrors.None;

            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, _encoding))
            {
                ShapeFileHeader header = null;
                try
                {
                    header = ShapeFileHeader.Read(reader);
                }
                catch (Exception exception)
                {
                    errors = errors.ShapeHeaderFormatError(entry.Name, exception);
                }

                if (header != null)
                {
                    errors = errors.CombineWith(_recordValidator.Validate(entry, header.CreateShapeRecordEnumerator(reader)));
                }
            }

            return errors;
        }
    }
}
