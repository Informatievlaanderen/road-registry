namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveDbaseEntryValidator<TRecord> : IZipArchiveEntryValidator
        where TRecord : DbaseRecord, new()
    {
        private readonly Encoding _encoding;
        private readonly DbaseSchema _schema;
        private readonly IZipArchiveDbaseRecordsValidator<TRecord> _recordValidator;

        public ZipArchiveDbaseEntryValidator(Encoding encoding, DbaseSchema schema, IZipArchiveDbaseRecordsValidator<TRecord> recordValidator)
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
                        errors = errors.CombineWith(_recordValidator.Validate(entry, new RecordEnumerator(reader)));
                    }
                }
            }

            return errors;
        }

        private class RecordEnumerator : IEnumerator<TRecord>
        {
            private readonly BinaryReader _reader;
            private TRecord _record;
            private bool _started;
            private bool _ended;

            public RecordEnumerator(BinaryReader reader)
            {
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
                _started = false;
                _ended = false;
            }

            public bool MoveNext()
            {
                if (_ended) { return false; }
                _started = true;

                var moved = false;
                try
                {
                    var record = new TRecord();
                    record.Read(_reader);
                    _record = record;
                    moved = true;
                }
                catch (EndOfStreamException)
                {
                    _ended = true;
                }
                catch (DbaseRecordException)
                {
                    _ended = true;
                }
                catch (Exception exception)
                {
                    _ended = true;
                    throw new DbaseRecordException($"There was a problem reading a {typeof(TRecord).Name}: {exception.Message}", exception);
                }

                return moved;
            }

            public void Reset()
            {
                throw new NotSupportedException("Enumeration can only be performed once.");
            }

            public TRecord Current
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
