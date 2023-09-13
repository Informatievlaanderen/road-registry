namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class RoadSegmentLaneChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadSegmentLaneChangeDbaseRecordsValidator _sut;

    public RoadSegmentLaneChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentPosition();
        _fixture.Customize<RoadSegmentLaneChangeDbaseRecord>(
            composer => composer
                .FromFactory(random =>
                {
                    var fromPosition = _fixture.Create<RoadSegmentPosition>().ToDouble();
                    return new RoadSegmentLaneChangeDbaseRecord
                    {
                        RECORDTYPE =
                        {
                            Value = (short)new Generator<RecordType>(_fixture).First(candidate =>
                                    candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed))
                                .Translation.Identifier
                        },
                        TRANSACTID = { Value = (short)random.Next(1, 9999) },
                        RS_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                        VANPOSITIE = { Value = fromPosition },
                        TOTPOSITIE = { Value = fromPosition + _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        AANTAL = { Value = (short)_fixture.Create<RoadSegmentLaneCount>().ToInt32() },
                        RICHTING =
                        {
                            Value = (short)_fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier
                        }
                    };
                })
                .OmitAutoProperties());

        _sut = new RoadSegmentLaneChangeDbaseRecordsValidator();
        _enumerator = new List<RoadSegmentLaneChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("attrijstroken_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.WS_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.RS_OIDN.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.RS_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.AANTAL.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.AANTAL
            };

            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.RICHTING.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.RICHTING
            };

            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.VANPOSITIE.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.VANPOSITIE
            };

            yield return new object[]
            {
                new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.TOTPOSITIE.Reset()),
                RoadSegmentLaneChangeDbaseRecord.Schema.TOTPOSITIE
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
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentLaneChangeDbaseRecord>>(_sut);
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
        var initialContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
            .Select(record =>
            {
                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                return record;
            })
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord>(records, 1, exception);

        var (result, context) = _sut.Validate(_entry, enumerator, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)
            ),
            result,
            new FileProblemComparer());
        Assert.Same(initialContext, context);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<RoadSegmentLaneChangeDbaseRecord> modifier, DbaseField field)
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
        modifier(record);
        if (record.WS_OIDN.HasValue)
        {
            initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
        }

        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Contains(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(field), result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.RS_OIDN.Value = index + 1;
                record.RECORDTYPE.Value = -1;
                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                return record;
            })
            .ToArray()
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

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
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierAndHaveAddedAndRemovedAsRecordTypeReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.RS_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                }

                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                return record;
            })
            .ToArray()
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.RS_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Identical.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                }

                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));

                return record;
            })
            .ToArray()
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
                    new AttributeId(1),
                    new RecordNumber(1))
            ),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.RS_OIDN.Value = 0;
                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                return record;
            })
            .ToArray()
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtDbaseRecord(new RecordNumber(1)).IdentifierZero(),
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierZero()
            ),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasFromAfterToPositionExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
        record.VANPOSITIE.Value = record.TOTPOSITIE.Value + 1;
        initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1))
                .FromPositionEqualToOrGreaterThanToPosition(record.VANPOSITIE.Value, record.TOTPOSITIE.Value)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidCountReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
        record.AANTAL.Value = -1;
        initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).LaneCountOutOfRange(-1)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidDirectionReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
        record.RICHTING.Value = -1;
        initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).LaneDirectionMismatch(-1)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
        record.WS_OIDN.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasSameFromAsToPositionExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
        record.VANPOSITIE.Value = record.TOTPOSITIE.Value;
        initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1))
                .FromPositionEqualToOrGreaterThanToPosition(record.VANPOSITIE.Value, record.TOTPOSITIE.Value)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentLaneChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.RS_OIDN.Value = index + 1;
                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                return record;
            })
            .ToArray()
            .ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Same(initialContext, context);
    }
}