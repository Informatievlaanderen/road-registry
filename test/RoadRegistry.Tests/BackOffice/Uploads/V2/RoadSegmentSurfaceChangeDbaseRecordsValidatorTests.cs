namespace RoadRegistry.Tests.BackOffice.Uploads.V2;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;

public class RoadSegmentSurfaceChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadSegmentSurfaceChangeDbaseRecordsValidator _sut;

    public RoadSegmentSurfaceChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentPosition();
        _fixture.Customize<RoadSegmentSurfaceChangeDbaseRecord>(
            composer => composer
                .FromFactory(random =>
                {
                    var fromPosition = _fixture.Create<RoadSegmentPosition>().ToDouble();
                    return new RoadSegmentSurfaceChangeDbaseRecord
                    {
                        RECORDTYPE =
                        {
                            Value = (short)new Generator<RecordType>(_fixture).First(candidate =>
                                    candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed))
                                .Translation.Identifier
                        },
                        TRANSACTID = { Value = (short)random.Next(1, 9999) },
                        WV_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                        WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                        VANPOSITIE = { Value = fromPosition },
                        TOTPOSITIE = { Value = fromPosition + _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TYPE = { Value = (short)_fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                    };
                })
                .OmitAutoProperties());

        _sut = new RoadSegmentSurfaceChangeDbaseRecordsValidator();
        _enumerator = new List<RoadSegmentSurfaceChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("attwegverharding_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                RoadSegmentSurfaceChangeDbaseRecord.Schema.WS_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                RoadSegmentSurfaceChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.WV_OIDN.Reset()),
                RoadSegmentSurfaceChangeDbaseRecord.Schema.WV_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.TYPE.Reset()),
                RoadSegmentSurfaceChangeDbaseRecord.Schema.TYPE
            };

            yield return new object[]
            {
                new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.VANPOSITIE.Reset()),
                RoadSegmentSurfaceChangeDbaseRecord.Schema.VANPOSITIE
            };

            yield return new object[]
            {
                new Action<RoadSegmentSurfaceChangeDbaseRecord>(r => r.TOTPOSITIE.Reset()),
                RoadSegmentSurfaceChangeDbaseRecord.Schema.TOTPOSITIE
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
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentSurfaceChangeDbaseRecord>>(_sut);
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
            .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
            .Select(record =>
            {
                initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                return record;
            })
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord>(records, 1, exception);

        var (result, context) = _sut.Validate(_entry, enumerator, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
            result,
            new FileProblemComparer());
        Assert.Same(initialContext, context);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<RoadSegmentSurfaceChangeDbaseRecord> modifier, DbaseField field)
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
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
            .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WV_OIDN.Value = index + 1;
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
            .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WV_OIDN.Value = 1;
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
            .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WV_OIDN.Value = 1;
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
            .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.WV_OIDN.Value = 0;
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
        var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
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
    public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
        record.WS_OIDN.Value = -1;
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidTypeReturnsExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
        record.TYPE.Value = -1;
        initialContext = initialContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, context) = _sut.Validate(_entry, records, initialContext);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).SurfaceTypeMismatch(-1)),
            result);
        Assert.Same(initialContext, context);
    }

    [Fact]
    public void ValidateWithRecordThatHasSameFromAsToPositionExpectedResult()
    {
        var initialContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentSurfaceChangeDbaseRecord>();
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
            .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WV_OIDN.Value = index + 1;
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