namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class RoadNodeChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadNodeChangeDbaseRecordsValidator _sut;

    public RoadNodeChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeType();
        _fixture.Customize<RoadNodeChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadNodeChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)random.Next(1, 5) },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WEGKNOOPID = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)_fixture.Create<RoadNodeType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        _sut = new RoadNodeChangeDbaseRecordsValidator();
        _enumerator = new List<RoadNodeChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("wegknoop_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<RoadNodeChangeDbaseRecord>(r => r.WEGKNOOPID.Reset()),
                RoadNodeChangeDbaseRecord.Schema.WEGKNOOPID
            };

            yield return new object[]
            {
                new Action<RoadNodeChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                RoadNodeChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<RoadNodeChangeDbaseRecord>(r => r.TYPE.Reset()),
                RoadNodeChangeDbaseRecord.Schema.TYPE
            };
        }
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    private static ZipArchiveValidationContext BuildValidationContext(RoadNodeChangeDbaseRecord record, ZipArchiveValidationContext context)
    {
        if (!record.WEGKNOOPID.HasValue || !record.RECORDTYPE.HasValue)
        {
            return context;
        }

        if (RecordType.ByIdentifier.TryGetValue(record.RECORDTYPE.Value, out var recordType))
        {
            return context.WithRoadNode(new RoadNodeId(record.WEGKNOOPID.Value), recordType);
        }

        return context;
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsValidator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadNodeChangeDbaseRecord>>(_sut);
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
        const int failAt = 1;
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadNodeChangeDbaseRecord>(2)
            .Select((record, i) =>
            {
                if (i < failAt)
                {
                    expectedContext = BuildValidationContext(record, expectedContext);
                }

                return record;
            })
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<RoadNodeChangeDbaseRecord>(records, failAt, exception);

        var (result, context) = _sut.Validate(_entry, enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
            result,
            new FileProblemComparer());
        Assert.Equal(expectedContext, context);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<RoadNodeChangeDbaseRecord> modifier, DbaseField field)
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadNodeChangeDbaseRecord>();
        modifier(record);
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Contains(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(field), result);
        Assert.Equal(expectedContext, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadNodeChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WEGKNOOPID.Value = index + 1;
                record.RECORDTYPE.Value = -1;
                expectedContext = BuildValidationContext(record, expectedContext);

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
        Assert.Equal(expectedContext, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameRoadNodeIdentifierButDifferentRecordTypeReturnsExpectedResult()
    {
        var record1 = _fixture.Create<RoadNodeChangeDbaseRecord>();
        var record2 = _fixture.Create<RoadNodeChangeDbaseRecord>();

        record1.WEGKNOOPID.Value = 1;
        record2.WEGKNOOPID.Value = 1;

        record1.RECORDTYPE.Value = RecordType.AddedIdentifier;
        record2.RECORDTYPE.Value = RecordType.RemovedIdentifier;

        var records = new[]
        {
            record1,
            record2
        }.ToDbaseRecordEnumerator();

        var (result, _) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUniqueButAllowed(
                    new RoadNodeId(1),
                    RecordType.Removed,
                    new RecordNumber(1),
                    RecordType.Added)
            ),
            result);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameRoadNodeIdentifierButSameRecordTypeReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<RoadNodeChangeDbaseRecord>(2)
            .Select((record, i) =>
            {
                record.WEGKNOOPID.Value = 1;
                record.RECORDTYPE.Value = RecordType.AddedIdentifier;
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, _) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
                    new RoadNodeId(1),
                    new RecordNumber(1))
            ),
            result);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveZeroAsRoadNodeIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<RoadNodeChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.WEGKNOOPID.Value = 0;
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
    public void ValidateWithRecordThatHasInvalidRoadNodeTypeReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadNodeChangeDbaseRecord>();
        record.TYPE.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();
        expectedContext = BuildValidationContext(record, expectedContext);

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadNodeTypeMismatch(-1)),
            result);
        Assert.Equal(expectedContext, context);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadNodeChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WEGKNOOPID.Value = index + 1;
                expectedContext = BuildValidationContext(record, expectedContext);
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Equal(expectedContext, context);
    }
}