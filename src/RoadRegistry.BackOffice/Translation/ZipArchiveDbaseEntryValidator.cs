namespace RoadRegistry.BackOffice.Translation
{
    using System;
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

        public ZipArchiveProblems Validate(ZipArchiveEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var fileContext = Problems.InFile(entry.Name);
            var problems = ZipArchiveProblems.None;

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
                    problems += fileContext.DbaseHeaderFormatError(exception);
                }

                if (header != null)
                {
                    if (!header.Schema.Equals(_schema))
                    {
                        problems += fileContext.DbaseSchemaMismatch(_schema, header.Schema);
                    }
                    else
                    {
                        using (var records = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader))
                        {
                            problems += _recordValidator.Validate(entry, records);
                        }
                    }
                }
            }

            return problems;
        }
    }
}
