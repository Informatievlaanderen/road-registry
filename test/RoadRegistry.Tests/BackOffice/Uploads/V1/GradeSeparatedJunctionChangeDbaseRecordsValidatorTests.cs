namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class GradeSeparatedJunctionChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly GradeSeparatedJunctionChangeDbaseRecordsValidator _sut;

    public GradeSeparatedJunctionChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeGradeSeparatedJunctionId();
        _fixture.CustomizeGradeSeparatedJunctionType();
        _fixture.Customize<GradeSeparatedJunctionChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)_fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                    BO_WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        _sut = new GradeSeparatedJunctionChangeDbaseRecordsValidator();
        _enumerator = new List<GradeSeparatedJunctionChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("rltogkruising_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<GradeSeparatedJunctionChangeDbaseRecord>(r => r.OK_OIDN.Reset()),
                GradeSeparatedJunctionChangeDbaseRecord.Schema.OK_OIDN
            };

            yield return new object[]
            {
                new Action<GradeSeparatedJunctionChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                GradeSeparatedJunctionChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<GradeSeparatedJunctionChangeDbaseRecord>(r => r.BO_WS_OIDN.Reset()),
                GradeSeparatedJunctionChangeDbaseRecord.Schema.BO_WS_OIDN
            };

            yield return new object[]
            {
                new Action<GradeSeparatedJunctionChangeDbaseRecord>(r => r.TYPE.Reset()),
                GradeSeparatedJunctionChangeDbaseRecord.Schema.TYPE
            };

            yield return new object[]
            {
                new Action<GradeSeparatedJunctionChangeDbaseRecord>(r => r.ON_WS_OIDN.Reset()),
                GradeSeparatedJunctionChangeDbaseRecord.Schema.ON_WS_OIDN
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
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<GradeSeparatedJunctionChangeDbaseRecord>>(_sut);
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
    }

    [Fact]
    public void ValidateWithProblematicRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord>(records, 1, exception);

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
        Action<GradeSeparatedJunctionChangeDbaseRecord> modifier, DbaseField field)
    {
        var record = _fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>();
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
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.OK_OIDN.Value = index + 1;
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
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.OK_OIDN.Value = 1;
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
    public void ValidateWithRecordsThatHaveTheSameGradeSeparatedJunctionIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.OK_OIDN.Value = 1;
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

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry
                    .AtDbaseRecord(new RecordNumber(2))
                    .IdentifierNotUnique(
                        new GradeSeparatedJunctionId(1),
                        new RecordNumber(1))),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveZeroAsGradeSeparatedJunctionIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.OK_OIDN.Value = 0;
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
    public void ValidateWithRecordThatHasInvalidGradeSeparatedJunctionTypeReturnsExpectedResult()
    {
        var record = _fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>();
        record.TYPE.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).GradeSeparatedJunctionTypeMismatch(-1)),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidLowerRoadSegmentIdReturnsExpectedResult()
    {
        var record = _fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>();
        record.ON_WS_OIDN.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).LowerRoadSegmentIdOutOfRange(-1)),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidUpperRoadSegmentIdReturnsExpectedResult()
    {
        var record = _fixture.Create<GradeSeparatedJunctionChangeDbaseRecord>();
        record.BO_WS_OIDN.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).UpperRoadSegmentIdOutOfRange(-1)),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.OK_OIDN.Value = index + 1;
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