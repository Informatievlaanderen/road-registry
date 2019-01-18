namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveDbaseEntryValidator : IZipArchiveEntryValidator
    {
        private readonly Encoding _encoding;
        private readonly DbaseSchema _schema;
        private readonly Func<IZipArchiveDbaseRecordValidator> _recordValidatorFactory;

        public ZipArchiveDbaseEntryValidator(Encoding encoding, DbaseSchema schema, Func<IZipArchiveDbaseRecordValidator> recordValidatorFactory)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _recordValidatorFactory = recordValidatorFactory ?? throw new ArgumentNullException(nameof(recordValidatorFactory));
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
                    errors = errors.DbaseHeaderFormatError(entry.Name.ToUpperInvariant(), exception);
                }

                if (header != null)
                {
                    if (header.RecordCount.Equals(new DbaseRecordCount(0)))
                    {
                        errors = errors.NoDbaseRecords(entry.Name.ToUpperInvariant());
                    }

                    if (!header.Schema.Equals(_schema))
                    {
                        //Report which columns are missing
                        //Report which columns do not match
                        //Report which columns are unexpected
                    }
                    else
                    {
                        var recordValidator = _recordValidatorFactory();
                        var continueReading = true;
                        var afterRecordNumber = new int?();
                        while (continueReading)
                        {
                            var record = header.CreateDbaseRecord();
                            try
                            {
                                if (reader.PeekChar() == DbaseRecord.EndOfFile)
                                {
                                    continueReading = false;
                                }
                                else
                                {
                                    record.Read(reader);
                                    if (afterRecordNumber.HasValue)
                                    {
                                        afterRecordNumber = afterRecordNumber.Value + 1;
                                    }
                                    else
                                    {
                                        afterRecordNumber = 1;
                                    }
                                }
                            }
                            catch (EndOfStreamException)
                            {
                                continueReading = false;
                            }
                            catch (Exception exception)
                            {
                                errors = errors.DbaseRecordFormatError(entry.Name.ToUpperInvariant(), afterRecordNumber, exception);
                                continueReading = false;
                            }

                            if (continueReading)
                            {
                                errors = errors.CombineWith(recordValidator.Validate(entry, record));
                            }
                        }
                    }
                }
            }

            return errors;
        }
    }
}
