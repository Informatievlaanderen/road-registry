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
                    errors = errors.CombineWith(_recordValidator.Validate(entry, new RecordEnumerator(header, reader)));
                }
            }

            return errors;
        }

        private class RecordEnumerator : IEnumerator<ShapeRecord>
        {
            private readonly ShapeFileHeader _header;
            private readonly BinaryReader _reader;
            private ShapeRecord _record;
            private bool _started;
            private bool _ended;
            private WordLength _lengthRead;

            public RecordEnumerator(ShapeFileHeader header, BinaryReader reader)
            {
                _header = header ?? throw new ArgumentNullException(nameof(header));
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
                _started = false;
                _ended = false;
                _lengthRead = ShapeFileHeader.Length;
            }

            public bool MoveNext()
            {
                if (_ended) { return false; }

                if (_lengthRead == _header.FileLength)
                {
                    _ended = true;
                    return false;
                }

                _started = true;

                var moved = false;
                try
                {
                    _record = ShapeRecord.Read(_reader);
                    _lengthRead = _lengthRead.Plus(_record.Length);
                    moved = true;
                }
                catch (EndOfStreamException)
                {
                    _ended = true;
                }
                catch (Exception exception)
                {
                    _ended = true;
                    //throw new ShapeRecordException($"There was a problem reading a ShapeRecord: {exception.Message}", exception);
                }

                return moved;
            }

            public void Reset()
            {
                throw new NotSupportedException("Enumeration can only be performed once.");
            }

            public ShapeRecord Current
            {
                get
                {
                    if (!_started)
                    {
                        throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                    }

                    if (_ended)
                    {
                        throw new InvalidOperationException("Enumeration has already ended. Reset is not supported.");
                    }

                    return _record;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
