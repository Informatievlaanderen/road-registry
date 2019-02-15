namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
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
        public void ValidateReturnsExpectedResultWhenEntryIsEmptyStream()
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
                        ZipArchiveErrors.None.DbaseHeaderFormatError(entry.Name, new EndOfStreamException()),
                        result);
                }
            }
        }
        private class FakeDbaseRecord : DbaseRecord {}

        private class FakeDbaseSchema : DbaseSchema {}
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
            }

            public int GetHashCode(Error obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
