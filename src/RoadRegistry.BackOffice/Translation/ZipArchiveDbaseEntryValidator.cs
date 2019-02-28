namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveDbaseEntryValidator<TDbaseRecord> : IZipArchiveEntryValidator
        where TDbaseRecord : DbaseRecord, new()
    {
        private readonly Encoding _encoding;
        private readonly DbaseSchema _schema;
        private readonly IZipArchiveDbaseRecordsValidator<TDbaseRecord> _recordValidator;

        public ZipArchiveDbaseEntryValidator(Encoding encoding, DbaseSchema schema, IZipArchiveDbaseRecordsValidator<TDbaseRecord> recordValidator)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public ZipArchiveErrors Validate(ZipArchiveEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            var errors = ZipArchiveErrors.None;

            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, _encoding))
            {
                DbaseFileHeader header = null;
                try
                {
                    header = DbaseFileHeader.Read(reader);
                }
                catch (Exception exception)
                {
                    errors = errors.DbaseHeaderFormatError(entry.Name, exception);
                }

                if (header != null)
                {
                    if (!header.Schema.Equals(_schema))
                    {
                        errors = errors.DbaseSchemaMismatch(entry.Name, _schema, header.Schema);
                    }
                    else
                    {
                        errors = errors.CombineWith(
                            _recordValidator.Validate(
                                entry,
                                header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader)));
                    }
                }
            }

            return errors;
        }
    }
}
