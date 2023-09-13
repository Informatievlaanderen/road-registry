namespace RoadRegistry.Tests.BackOffice.Uploads.V2;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

public class EuropeanRoadChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly EuropeanRoadChangeDbaseRecordsValidator _sut;

    public EuropeanRoadChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeEuropeanRoadNumber();
        _fixture.Customize<EuropeanRoadChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new EuropeanRoadChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    EU_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    EUNUMMER = { Value = _fixture.Create<EuropeanRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        _sut = new EuropeanRoadChangeDbaseRecordsValidator();
        _enumerator = new List<EuropeanRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("atteuropweg_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<EuropeanRoadChangeDbaseRecord>(r => r.EU_OIDN.Reset()),
                EuropeanRoadChangeDbaseRecord.Schema.EU_OIDN
            };

            yield return new object[]
            {
                new Action<EuropeanRoadChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                EuropeanRoadChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<EuropeanRoadChangeDbaseRecord>(r => r.EUNUMMER.Reset()),
                EuropeanRoadChangeDbaseRecord.Schema.EUNUMMER
            };

            yield return new object[]
            {
                new Action<EuropeanRoadChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                EuropeanRoadChangeDbaseRecord.Schema.WS_OIDN
            };
        }
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsValidator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<EuropeanRoadChangeDbaseRecord>>(_sut);
    }

    [Fact]
    public void ValidateContextCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, _enumerator, null));
    }

    [Fact]
    public void ValidateEntryCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, _enumerator, _context));
    }

    [Fact]
    public void ValidateRecordsCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null, _context));
    }

    [Fact]
    public void ValidateWithoutRecordsReturnsExpectedResult()
    {
        var (result, context) = _sut.Validate(_entry, _enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.HasNoDbaseRecords()),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithProblematicRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord>(records, 1, exception);

        var (result, context) = _sut.Validate(_entry, enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)
            ),
            result,
            new FileProblemComparer());
        Assert.Same(_context, context);
    }

    [Theory]
    [InlineData("")]
    [InlineData("X40")]
    public void ValidateWithRecordsThatDoNotHaveEuropeanRoadNumbersReturnsExpectedResult(string number)
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = index + 1;
                record.EUNUMMER.Value = number;
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtDbaseRecord(new RecordNumber(1)).NotEuropeanRoadNumber(number),
                _entry.AtDbaseRecord(new RecordNumber(2)).NotEuropeanRoadNumber(number)
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveNullAsEuropeanRoadNumbersReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = index + 1;
                record.EUNUMMER.Value = null;
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(EuropeanRoadChangeDbaseRecord.Schema.EUNUMMER),
                _entry.AtDbaseRecord(new RecordNumber(2)).RequiredFieldIsNull(EuropeanRoadChangeDbaseRecord.Schema.EUNUMMER)
            ),
            result);
        Assert.Same(_context, context);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<EuropeanRoadChangeDbaseRecord> modifier, DbaseField field)
    {
        var record = _fixture.Create<EuropeanRoadChangeDbaseRecord>();
        modifier(record);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Contains(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(field), result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = index + 1;
                record.RECORDTYPE.Value = -1;
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry
                    .AtDbaseRecord(new RecordNumber(1))
                    .RecordTypeMismatch(-1),
                _entry
                    .AtDbaseRecord(new RecordNumber(2))
                    .RecordTypeMismatch(-1)
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierAndHaveAddedAndRemovedAsRecordTypeReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                }

                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierButNotRecordTypeAddedReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Identical.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                }

                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(ZipArchiveProblems.None, result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                }

                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry
                    .AtDbaseRecord(new RecordNumber(2))
                    .IdentifierNotUnique(new AttributeId(1), new RecordNumber(1))
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.EU_OIDN.Value = 0;
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtDbaseRecord(new RecordNumber(1)).IdentifierZero(),
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierZero()
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidEuropeanRoadNumberReturnsExpectedResult()
    {
        var record = _fixture.Create<EuropeanRoadChangeDbaseRecord>();
        record.EUNUMMER.Value = "-1";
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NotEuropeanRoadNumber("-1")),
            result);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
    {
        var record = _fixture.Create<EuropeanRoadChangeDbaseRecord>();
        record.WS_OIDN.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<EuropeanRoadChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.EU_OIDN.Value = index + 1;
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Same(_context, context);
    }
}