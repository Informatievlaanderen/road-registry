namespace RoadRegistry.Tests.BackOffice.Uploads.V2;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

public class NationalRoadChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly NationalRoadChangeDbaseRecordsValidator _sut;

    public NationalRoadChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeNationalRoadNumber();
        _fixture.Customize<NationalRoadChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new NationalRoadChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT2 = { Value = _fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        _sut = new NationalRoadChangeDbaseRecordsValidator();
        _enumerator = new List<NationalRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("attnationweg_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<NationalRoadChangeDbaseRecord>(r => r.NW_OIDN.Reset()),
                NationalRoadChangeDbaseRecord.Schema.NW_OIDN
            };

            yield return new object[]
            {
                new Action<NationalRoadChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                NationalRoadChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<NationalRoadChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                NationalRoadChangeDbaseRecord.Schema.WS_OIDN
            };

            yield return new object[]
            {
                new Action<NationalRoadChangeDbaseRecord>(r => r.IDENT2.Reset()),
                NationalRoadChangeDbaseRecord.Schema.IDENT2
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
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<NationalRoadChangeDbaseRecord>>(_sut);
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
            .CreateMany<NationalRoadChangeDbaseRecord>(2)
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<NationalRoadChangeDbaseRecord>(records, 1, exception);

        var (result, context) = _sut.Validate(_entry, enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
            result,
            new FileProblemComparer());
        Assert.Same(_context, context);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<NationalRoadChangeDbaseRecord> modifier, DbaseField field)
    {
        var record = _fixture.Create<NationalRoadChangeDbaseRecord>();
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
            .CreateMany<NationalRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.NW_OIDN.Value = index + 1;
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
            .CreateMany<NationalRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.NW_OIDN.Value = 1;
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
            .CreateMany<NationalRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.NW_OIDN.Value = 1;
                switch (index % 2)
                {
                    case 0:
                        record.RECORDTYPE.Value = (short)RecordType.Identical.Translation.Identifier;
                        break;
                    case 1:
                        record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                        break;
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
            .CreateMany<NationalRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.NW_OIDN.Value = 1;
                switch (index % 2)
                {
                    case 0:
                        record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                        break;
                    case 1:
                        record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                        break;
                }

                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2))
                    .IdentifierNotUnique(
                        new AttributeId(1),
                        new RecordNumber(1))
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<NationalRoadChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.NW_OIDN.Value = 0;
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
    public void ValidateWithRecordThatHasInvalidNationalRoadNumberReturnsExpectedResult()
    {
        var record = _fixture.Create<NationalRoadChangeDbaseRecord>();
        record.IDENT2.Value = "-1";
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NotNationalRoadNumber("-1")),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
    {
        var record = _fixture.Create<NationalRoadChangeDbaseRecord>();
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
            .CreateMany<NationalRoadChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.NW_OIDN.Value = index + 1;
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