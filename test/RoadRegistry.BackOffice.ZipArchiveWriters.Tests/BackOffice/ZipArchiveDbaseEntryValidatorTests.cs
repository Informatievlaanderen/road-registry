namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using RoadRegistry.Tests.BackOffice.Uploads;
using Uploads;

public class ZipArchiveDbaseEntryValidatorTests
{
    private readonly ZipArchiveValidationContext _context;

    public ZipArchiveDbaseEntryValidatorTests()
    {
        _context = ZipArchiveValidationContext.Empty;
    }

    private class CollectDbaseRecordValidator : IZipArchiveDbaseRecordsValidator<FakeDbaseRecord>
    {
        public FakeDbaseRecord[] Collected { get; private set; }

        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<FakeDbaseRecord> records, ZipArchiveValidationContext context)
        {
            var collected = new List<FakeDbaseRecord>();
            while (records.MoveNext()) collected.Add(records.Current);
            Collected = collected.ToArray();

            return (ZipArchiveProblems.None, context);
        }
    }

    [Fact]
    public void EncodingCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                null, DbaseFileHeaderReadBehavior.Default,
                new FakeDbaseSchema(),
                new FakeDbaseRecordValidator()));
    }

    private class FakeDbaseRecord : DbaseRecord
    {
        public FakeDbaseRecord()
        {
            Field = new DbaseNumber(Schema.Field);
            Values = new DbaseFieldValue[] { Field };
        }

        public override bool Equals(object obj)
        {
            return obj is FakeDbaseRecord other && Equals(other);
        }

        public bool Equals(FakeDbaseRecord other)
        {
            return other != null && Field.Field.Equals(other.Field.Field) &&
                   Field.Value.Equals(other.Field.Value);
        }

        public DbaseNumber Field { get; }

        public override int GetHashCode()
        {
            return Field.GetHashCode();
        }

        private static readonly FakeDbaseSchema Schema = new();
    }

    private class FakeDbaseRecordValidator : IZipArchiveDbaseRecordsValidator<FakeDbaseRecord>
    {
        private readonly FileProblem[] _problems;

        public FakeDbaseRecordValidator(params FileProblem[] problems)
        {
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<FakeDbaseRecord> records, ZipArchiveValidationContext context)
        {
            return (ZipArchiveProblems.None.AddRange(_problems), context);
        }
    }

    private class FakeDbaseSchema : DbaseSchema
    {
        public FakeDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(Field)),
                    new DbaseFieldLength(10),
                    new DbaseDecimalCount(0))
            };
        }

        public DbaseField Field => Fields[0];
    }

    [Fact]
    public void SchemaCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.Default, DbaseFileHeaderReadBehavior.Default,
                null,
                new FakeDbaseRecordValidator()));
    }

    [Fact]
    public void ValidateContextCanNotBeNull()
    {
        var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
            Encoding.Default, DbaseFileHeaderReadBehavior.Default,
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
                Assert.Throws<ArgumentNullException>(() => sut.Validate(archive.GetEntry("entry"), null));
            }
        }
    }

    [Fact]
    public void ValidateEntryCanNotBeNull()
    {
        var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
            Encoding.Default, DbaseFileHeaderReadBehavior.Default,
            new FakeDbaseSchema(),
            new FakeDbaseRecordValidator());

        Assert.Throws<ArgumentNullException>(() => sut.Validate(null, _context));
    }

    [Fact]
    public void ValidatePassesExpectedDbaseRecordsToDbaseRecordValidator()
    {
        var schema = new FakeDbaseSchema();
        var validator = new CollectDbaseRecordValidator();
        var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
            Encoding.UTF8, DbaseFileHeaderReadBehavior.Default,
            schema,
            validator);
        var records = new[]
        {
            new FakeDbaseRecord { Field = { Value = 1 } },
            new FakeDbaseRecord { Field = { Value = 2 } }
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
                using (var entryStream = entry.Open())
                using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                {
                    header.Write(writer);
                    foreach (var record in records) record.Write(writer);
                    writer.Write(DbaseRecord.EndOfFile);
                    entryStream.Flush();
                }
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = archive.GetEntry("entry");

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(ZipArchiveProblems.None, result);
                Assert.Equal(records, validator.Collected);
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultWhenDbaseFileHeaderDoesNotMatch()
    {
        var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
            Encoding.UTF8, DbaseFileHeaderReadBehavior.Default,
            new FakeDbaseSchema(),
            new FakeDbaseRecordValidator());
        var date = DateTime.Today;
        var header = new DbaseFileHeader(
            date,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(0),
            new AnonymousDbaseSchema(Array.Empty<DbaseField>()));

        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("entry");
                using (var entryStream = entry.Open())
                using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
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

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.Single(entry.HasDbaseSchemaMismatch(
                        new FakeDbaseSchema(),
                        new AnonymousDbaseSchema(Array.Empty<DbaseField>()))
                    ),
                    result);
                Assert.Same(_context, context);
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
            Encoding.UTF8, DbaseFileHeaderReadBehavior.Default,
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
                using (var entryStream = entry.Open())
                using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
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

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.None.AddRange(problems),
                    result);
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultWhenEntryStreamContainsMalformedDbaseHeader()
    {
        var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
            Encoding.Default, DbaseFileHeaderReadBehavior.Default,
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

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.Single(entry.HasDbaseHeaderFormatError(
                        new DbaseFileHeaderException("The database file type must be 3 (dBase III)."))
                    ),
                    result,
                    new FileProblemComparer());
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultWhenEntryStreamIsEmpty()
    {
        var sut = new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
            Encoding.Default, DbaseFileHeaderReadBehavior.Default,
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

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.Single(entry.HasDbaseHeaderFormatError(
                        new EndOfStreamException("Unable to read beyond the end of the stream."))
                    ),
                    result,
                    new FileProblemComparer());
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidatorCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ZipArchiveDbaseEntryValidator<FakeDbaseRecord>(
                Encoding.Default, DbaseFileHeaderReadBehavior.Default,
                new FakeDbaseSchema(),
                null));
    }
}