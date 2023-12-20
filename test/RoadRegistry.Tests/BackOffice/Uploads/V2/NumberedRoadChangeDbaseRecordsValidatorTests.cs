namespace RoadRegistry.Tests.BackOffice.Uploads.V2;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

public class NumberedRoadChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly NumberedRoadChangeDbaseRecordsValidator _sut;

    public NumberedRoadChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeNumberedRoadNumber();
        _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        _fixture.CustomizeRoadSegmentNumberedRoadDirection();
        _fixture.Customize<NumberedRoadChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new NumberedRoadChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    GW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT8 = { Value = _fixture.Create<NumberedRoadNumber>().ToString() },
                    RICHTING = { Value = (short)_fixture.Create<RoadSegmentNumberedRoadDirection>().Translation.Identifier },
                    VOLGNUMMER = { Value = _fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32() }
                })
                .OmitAutoProperties());

        _sut = new NumberedRoadChangeDbaseRecordsValidator();
        _enumerator = new List<NumberedRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("attgenumweg_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<NumberedRoadChangeDbaseRecord>(r => r.GW_OIDN.Reset()),
                NumberedRoadChangeDbaseRecord.Schema.GW_OIDN
            };

            yield return new object[]
            {
                new Action<NumberedRoadChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                NumberedRoadChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<NumberedRoadChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                NumberedRoadChangeDbaseRecord.Schema.WS_OIDN
            };

            yield return new object[]
            {
                new Action<NumberedRoadChangeDbaseRecord>(r => r.IDENT8.Reset()),
                NumberedRoadChangeDbaseRecord.Schema.IDENT8
            };

            yield return new object[]
            {
                new Action<NumberedRoadChangeDbaseRecord>(r => r.RICHTING.Reset()),
                NumberedRoadChangeDbaseRecord.Schema.RICHTING
            };

            yield return new object[]
            {
                new Action<NumberedRoadChangeDbaseRecord>(r => r.VOLGNUMMER.Reset()),
                NumberedRoadChangeDbaseRecord.Schema.VOLGNUMMER
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
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<NumberedRoadChangeDbaseRecord>>(_sut);
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(2)
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord>(records, 1, exception);

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
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<NumberedRoadChangeDbaseRecord> modifier, DbaseField field)
    {
        var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.GW_OIDN.Value = index + 1;
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.GW_OIDN.Value = 1;
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.GW_OIDN.Value = 1;
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.GW_OIDN.Value = 1;
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
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.GW_OIDN.Value = 0;
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
    public void ValidateWithRecordThatHasInvalidNumberedRoadDirectionReturnsExpectedResult()
    {
        var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
        record.RICHTING.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NumberedRoadDirectionMismatch(-1)),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidNumberedRoadNumberReturnsExpectedResult()
    {
        var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
        record.IDENT8.Value = "-1";
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NotNumberedRoadNumber("-1")),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidNumberedRoadOrdinalReturnsExpectedResult()
    {
        var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
        record.VOLGNUMMER.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NumberedRoadOrdinalOutOfRange(-1)),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
    {
        var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
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
            .CreateMany<NumberedRoadChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.GW_OIDN.Value = index + 1;
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