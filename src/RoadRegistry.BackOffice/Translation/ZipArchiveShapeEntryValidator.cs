//namespace RoadRegistry.BackOffice.Translation
//{
//    using System;
//    using System.IO;
//    using System.IO.Compression;
//    using System.Text;
//    using Be.Vlaanderen.Basisregisters.Shaperon;
//
//    public class ZipArchiveShapeEntryValidator : IZipArchiveEntryValidator
//    {
//        private readonly Encoding _encoding;
//        private readonly IZipArchiveShapeRecordsValidator _recordValidator;
//
//        public ZipArchiveShapeEntryValidator(Encoding encoding, IZipArchiveShapeRecordsValidator recordValidator)
//        {
//            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
//            _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
//        }
//
//        public ZipArchiveErrors Validate(ZipArchiveEntry entry)
//        {
//            if (entry == null) throw new ArgumentNullException(nameof(entry));
//
//            var errors = ZipArchiveErrors.None;
//
//            using (var stream = entry.Open())
//            using (var reader = new BinaryReader(stream, _encoding))
//            {
//                ShapeFileHeader header = null;
//                try
//                {
//                    header = ShapeFileHeader.Read(reader);
//                }
//                catch (Exception exception)
//                {
//                    errors = errors.ShapeHeaderFormatError(entry.Name, exception);
//                }
//
//                if (header != null)
//                {
//                    var recordValidator = _recordValidatorFactory();
//                    // REMARK (Yves)
//                    // Indicates if we can continue reading from the underlying stream
//                    var continueReading = true;
//                    var recordCount = 0;
//                    // REMARK (Yves)
//                    // Denotes the record after which we get an exception during reading,
//                    // which should ease diagnostics
//                    var afterRecordNumber = new RecordNumber?();
//                    var readLength = ShapeFileHeader.Length;
//                    // REMARK (Yves)
//                    // Read until we can no longer read (due to eos or exception during read) or
//                    // the read length becomes bigger than the file length according to the
//                    // shape file header
//                    while (continueReading && readLength < header.FileLength)
//                    {
//                        ShapeRecord record = null;
//                        try
//                        {
//                            record = ShapeRecord.Read(reader);
//                            readLength = readLength.Plus(record.Length);
//                            afterRecordNumber = record.Header.RecordNumber;
//                            recordCount++;
//                        }
//                        catch (EndOfStreamException)
//                        {
//                            continueReading = false;
//                        }
//                        catch (Exception exception)
//                        {
//                            errors = errors.ShapeRecordFormatError(entry.Name, afterRecordNumber, exception);
//                            continueReading = false;
//                        }
//
//                        if (record != null)
//                        {
//                            errors = errors.CombineWith(recordValidator.Validate(entry, record));
//                        }
//                    }
//
//                    if (recordCount == 0)
//                    {
//                        errors = errors.NoShapeRecords(entry.Name);
//                    }
//                }
//            }
//
//            return errors;
//        }
//    }
//}
