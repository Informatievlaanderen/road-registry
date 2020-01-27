namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
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
                        ZipArchiveProblems.Single(entry.HasDbaseHeaderFormatError(
                            new EndOfStreamException("Unable to read beyond the end of the stream."))
                        ),
                        result,
                        new FileProblemComparer());
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
                        ZipArchiveProblems.Single(entry.HasDbaseHeaderFormatError(
                            new DbaseFileHeaderException("The database file type must be 3 (dBase III)."))
                        ),
                        result,
                        new FileProblemComparer());
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
                        ZipArchiveProblems.Single(entry.HasDbaseSchemaMismatch(
                            new FakeDbaseSchema(),
                            new AnonymousDbaseSchema(new DbaseField[0]))
                        ),
                        result);
                }
            }
        }

        [Fact]
        public void ValidateReturnsExpectedResultWhenDbaseRecordValidatorReturnsErrors()
        {
            var problems = new FileProblem[]
            {
                new FileError("file1", "error1", new ProblemParameter("parameter1", "value1")),
                new FileWarning("file2", "error2", new ProblemParameter("parameter2", "value2"))
            };
            var schema = new FakeDbaseSchema();
            var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.UTF8,
                schema,
                new FakeDbaseRecordValidator(problems));
            var date = DateTime.Today;
            var header = new DbaseFileHeader(
                date,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(0),
                schema);

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
                        ZipArchiveProblems.None.AddRange(problems),
                        result);
                }
            }
        }

        [Fact]
        public void ValidatePassesExpectedDbaseRecordsToDbaseRecordValidator()
        {
            var schema = new FakeDbaseSchema();
            var validator = new CollectDbaseRecordValidator();
            var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.UTF8,
                schema,
                validator);
            var records = new []
            {
                new FakeDbaseRecord {Field = {Value = 1}},
                new FakeDbaseRecord {Field = {Value = 2}}
            };
            var date = DateTime.Today;
            var header = new DbaseFileHeader(
                date,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(records.Length),
                schema);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("entry");
                    using(var entryStream = entry.Open())
                    using(var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                    {
                        header.Write(writer);
                        foreach (var record in records)
                        {
                            record.Write(writer);
                        }
                        writer.Write(DbaseRecord.EndOfFile);
                        entryStream.Flush();
                    }
                }
                stream.Flush();
                stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                {
                    var entry = archive.GetEntry("entry");

                    var result = sut.Validate(entry);

                    Assert.Equal(ZipArchiveProblems.None, result);
                    Assert.Equal(records, validator.Collected);
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

            public bool Equals(FakeDbaseRecord other) => other != null && Field.Field.Equals(other.Field.Field) &&
                                                         Field.Value.Equals(other.Field.Value);
            public override bool Equals(object obj) => obj is FakeDbaseRecord other && Equals(other);
            public override int GetHashCode() => Field.GetHashCode();
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
            private readonly FileProblem[] _problems;

            public FakeDbaseRecordValidator(params FileProblem[] problems)
            {
                _problems = problems ?? throw new ArgumentNullException(nameof(problems));
            }

            public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<FakeDbaseRecord> records)
            {
                return ZipArchiveProblems.None.AddRange(_problems);
            }
        }

        private class CollectDbaseRecordValidator : IZipArchiveDbaseRecordsValidator<FakeDbaseRecord>
        {
            public ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<FakeDbaseRecord> records)
            {
                var collected = new List<FakeDbaseRecord>();
                while (records.MoveNext())
                {
                    collected.Add(records.Current);
                }
                Collected = collected.ToArray();

                return ZipArchiveProblems.None;
            }

            public FakeDbaseRecord[] Collected { get; private set; }
        }
    }
}
