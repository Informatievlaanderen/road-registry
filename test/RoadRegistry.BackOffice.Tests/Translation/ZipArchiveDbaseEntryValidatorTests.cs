namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using Xunit;

    public class ZipArchiveDbaseEntryValidatorTests
    {
        [Fact]
        public void EncodingCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                    null,
                    new FakeDbaseSchema(),
                    new FakeDbaseRecordValidator()));
        }

        [Fact]
        public void SchemaCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                    Encoding.Default,
                    null,
                    new FakeDbaseRecordValidator()));
        }

        [Fact]
        public void ValidatorCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                    Encoding.Default,
                    new FakeDbaseSchema(),
                    null));
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.Default,
                new FakeDbaseSchema(),
                new FakeDbaseRecordValidator());

            Assert.Throws<ArgumentNullException>(() => sut.Validate(null));
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenEntryStreamIsEmpty()
        {
            var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.Default,
                new FakeDbaseSchema(),
                new FakeDbaseRecordValidator());

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry("entry");
                }
                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(
                        ZipArchiveErrors.None.DbaseHeaderFormatError(
                            entry.Name,
                            new EndOfStreamException("Unable to read beyond the end of the stream.")),
                        result,
                        new ErrorComparer());
                }
            }
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenEntryStreamContainsMalformedDbaseHeader()
        {
            var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.Default,
                new FakeDbaseSchema(),
                new FakeDbaseRecordValidator());

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using (var entryStream = entry.Open())
                    {
                        entryStream.WriteByte(0);
                        entryStream.Flush();
                    }
                }
                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(
                        ZipArchiveErrors.None.DbaseHeaderFormatError(
                            entry.Name,
                            new DbaseFileHeaderException("The database file type must be 3 (dBase III).")),
                        result,
                        new ErrorComparer());
                }
            }
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenDbaseFileHeaderDoesNotMatch()
        {
            var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.UTF8,
                new FakeDbaseSchema(),
                new FakeDbaseRecordValidator());
            var date = DateTime.Today;
            var header = new DbaseFileHeader(
                date,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(0),
                new AnonymousDbaseSchema(new DbaseField[0]));

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using(var entryStream = entry.Open())
                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                    {
                        header.Write(writer);
                        entryStream.Flush();
                    }
                }
                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(
                        ZipArchiveErrors.None.DbaseSchemaMismatch(
                            entry.Name,
                            new FakeDbaseSchema(),
                            new AnonymousDbaseSchema(new DbaseField[0])),
                        result);
                }
            }
        }

        private class FakeDbaseRecord : DbaseRecord
        {
            private static readonly FakeDbaseSchema Schema = new FakeDbaseSchema();

            public FakeDbaseRecord()
            {
                Field = new DbaseInt32(Schema.Field);
                Values = new DbaseFieldValue[] { Field };
            }

            public DbaseInt32 Field { get; }
        }

        private class FakeDbaseSchema : DbaseSchema
        {
            public FakeDbaseSchema()
            {
                Fields = new []
                {
                    DbaseField.CreateInt32Field(
                        new DbaseFieldName(nameof(Field)),
                        new DbaseFieldLength(10))
                };
            }

            public DbaseField Field => Fields[0];
        }

        private class FakeDbaseRecordValidator : IZipArchiveDbaseRecordsValidator<FakeDbaseRecord>
        {
            public ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<FakeDbaseRecord> records)
            {
                return ZipArchiveErrors.None;
            }
        }

        private class ErrorComparer : IEqualityComparer<Error>
        {
            public bool Equals(Error expected, Error actual)
            {
                if (expected == null && actual == null) return true;
                if (expected == null || actual == null) return false;
                return expected.Reason.Equals(actual.Reason) &&
                       expected.Parameters.SequenceEqual(actual.Parameters, new ProblemParameterComparer());

            }

            public int GetHashCode(Error obj)
            {
                return obj.GetHashCode();
            }
        }

        private class ProblemParameterComparer : IEqualityComparer<ProblemParameter>
        {
            public bool Equals(ProblemParameter expected, ProblemParameter actual)
            {
                if (expected == null && actual == null) return true;
                if (expected == null || actual == null) return false;
                return expected.Name.Equals(actual.Name) &&
                       actual.Value.Contains(expected.Value);
            }

            public int GetHashCode(ProblemParameter obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
