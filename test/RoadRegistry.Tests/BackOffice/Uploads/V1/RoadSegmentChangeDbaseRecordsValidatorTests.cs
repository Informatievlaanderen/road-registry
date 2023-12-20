namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class RoadSegmentChangeDbaseRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadSegmentChangeDbaseRecordsValidator _sut;

    public RoadSegmentChangeDbaseRecordsValidatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.Customize<RoadSegmentChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                    METHODE = { Value = (short)_fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier },
                    BEHEERDER = { Value = _fixture.Create<OrganizationId>() },
                    MORFOLOGIE = { Value = (short)_fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                    STATUS = { Value = _fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                    CATEGORIE = { Value = _fixture.Create<RoadSegmentCategory>().Translation.Identifier },
                    B_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    E_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    LSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    RSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue)) },
                    TGBEP = { Value = (short)_fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier },
                    EVENTIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) }
                })
                .OmitAutoProperties());

        _sut = new RoadSegmentChangeDbaseRecordsValidator();
        _enumerator = new List<RoadSegmentChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("wegsegment_all.dbf");
        _context = ZipArchiveValidationContext.Empty;
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveEmptyStringAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.BEHEERDER.Value = string.Empty),
                RoadSegmentChangeDbaseRecord.Schema.BEHEERDER
            };
        }
    }

    public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
    {
        get
        {
            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.WS_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.RECORDTYPE
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.B_WK_OIDN.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.E_WK_OIDN.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.TGBEP.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.TGBEP
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.STATUS.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.STATUS
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.CATEGORIE.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.CATEGORIE
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.MORFOLOGIE.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.METHODE.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.METHODE
            };

            yield return new object[]
            {
                new Action<RoadSegmentChangeDbaseRecord>(r => r.BEHEERDER.Reset()),
                RoadSegmentChangeDbaseRecord.Schema.BEHEERDER
            };
        }
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    private static ZipArchiveValidationContext BuildValidationContext(
        RoadSegmentChangeDbaseRecord record,
        ZipArchiveValidationContext expectedContext)
    {
        if (record.RECORDTYPE.HasValue)
        {
            switch (record.RECORDTYPE.Value)
            {
                case RecordType.IdenticalIdentifier:
                    if (record.WS_OIDN.HasValue && RoadSegmentId.Accepts(record.WS_OIDN.Value))
                    {
                        expectedContext =
                            expectedContext.WithIdenticalRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                    }

                    break;
                case RecordType.AddedIdentifier:
                    if (record.WS_OIDN.HasValue)
                    {
                        if (record.EVENTIDN.HasValue && record.EVENTIDN.Value != 0)
                        {
                            expectedContext =
                                expectedContext.WithAddedRoadSegment(new RoadSegmentId(record.EVENTIDN.Value));
                        }
                        else if (RoadSegmentId.Accepts(record.WS_OIDN.Value))
                        {
                            expectedContext =
                                expectedContext.WithAddedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                        }
                    }

                    break;
                case RecordType.ModifiedIdentifier:
                    if (record.WS_OIDN.HasValue && RoadSegmentId.Accepts(record.WS_OIDN.Value))
                    {
                        expectedContext =
                            expectedContext.WithModifiedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                    }

                    break;
                case RecordType.RemovedIdentifier:
                    if (record.WS_OIDN.HasValue && RoadSegmentId.Accepts(record.WS_OIDN.Value))
                    {
                        expectedContext =
                            expectedContext.WithRemovedRoadSegment(new RoadSegmentId(record.WS_OIDN.Value));
                    }

                    break;
            }
        }

        return expectedContext;
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsValidator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentChangeDbaseRecord>>(_sut);
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
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                if (index == 0)
                {
                    expectedContext = BuildValidationContext(record, expectedContext);
                }

                return record;
            })
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord>(records, 1, exception);

        var (result, actualContext) = _sut.Validate(_entry, enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
            result,
            new FileProblemComparer());
        Assert.Equal(expectedContext, actualContext);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveEmptyStringAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveEmptyStringAsRequiredFieldValueReturnsExpectedResult(
        Action<RoadSegmentChangeDbaseRecord> modifier, DbaseField field)
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        modifier(record);
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Contains(_entry
            .AtDbaseRecord(new RecordNumber(1))
            .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
            .RequiredFieldIsNull(field), result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Theory]
    [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
    public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
        Action<RoadSegmentChangeDbaseRecord> modifier, DbaseField field)
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        modifier(record);
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Contains(_entry
            .AtDbaseRecord(new RecordNumber(1))
            .RequiredFieldIsNull(field), result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WS_OIDN.Value = index + 1;
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
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WS_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                }

                expectedContext = BuildValidationContext(record, expectedContext);

                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveTheSameRoadSegmentIdentifierReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(2)
            .Select((record, index) =>
            {
                record.WS_OIDN.Value = 1;
                if (index == 0)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Identical.Translation.Identifier;
                }
                else if (index == 1)
                {
                    record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                }

                expectedContext = BuildValidationContext(record, expectedContext);
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
                    new RoadSegmentId(1),
                    new RecordNumber(1))
            ),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordsThatHaveZeroAsRoadSegmentIdentifierReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(2)
            .Select(record =>
            {
                record.WS_OIDN.Value = 0;
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
    public void ValidateWithRecordThatHasInvalidAccessRestrictionReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.TGBEP.Value = -1;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .RoadSegmentAccessRestrictionMismatch(-1)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidBeginRoadNodeIdReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.B_WK_OIDN.Value = -1;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .BeginRoadNodeIdOutOfRange(-1)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidCategoryReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.CATEGORIE.Value = "-1";
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .RoadSegmentCategoryMismatch("-1")),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidEndRoadNodeIdReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.E_WK_OIDN.Value = -1;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .EndRoadNodeIdOutOfRange(-1)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidGeometryDrawMethodReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.METHODE.Value = -1;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .RoadSegmentGeometryDrawMethodMismatch(-1)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidLeftStreetNameIdReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.LSTRNMID.Value = -12;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .LeftStreetNameIdOutOfRange(-12)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidMorphologyReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.MORFOLOGIE.Value = -1;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .RoadSegmentMorphologyMismatch(-1)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidRightStreetNameIdReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.RSTRNMID.Value = -12;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .RightStreetNameIdOutOfRange(-12)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasInvalidStatusReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.STATUS.Value = -1;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .RoadSegmentStatusMismatch(-1)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithRecordThatHasSameBeginAndEndRoadNodeId()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
        record.B_WK_OIDN.Value = record.E_WK_OIDN.Value;
        expectedContext = BuildValidationContext(record, expectedContext);
        var records = new[] { record }.ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry
                .AtDbaseRecord(new RecordNumber(1))
                .WithIdentifier("WS_OIDN", record.WS_OIDN.GetValue())
                .BeginRoadNodeIdEqualsEndRoadNode(record.B_WK_OIDN.Value, record.E_WK_OIDN.Value)),
            result);
        Assert.Equal(expectedContext, actualContext);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var expectedContext = ZipArchiveValidationContext.Empty;
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WS_OIDN.Value = index + 1;
                expectedContext = BuildValidationContext(record, expectedContext);
                return record;
            })
            .ToDbaseRecordEnumerator();

        var (result, actualContext) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Equal(expectedContext, actualContext);
    }
}
