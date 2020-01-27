namespace RoadRegistry.BackOffice.Uploads
{
    using System;
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

        public ZipArchiveProblems Validate(ZipArchiveEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var problems = ZipArchiveProblems.None;

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
                    problems += entry.HasShapeHeaderFormatError(exception);
                }

                if (header != null)
                {
                    using (var records = header.CreateShapeRecordEnumerator(reader))
                    {
                        problems += _recordValidator.Validate(entry, records);
                    }
                }
            }

            return problems;
        }
    }
}
