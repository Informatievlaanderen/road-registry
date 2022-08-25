namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Schema;
using Xunit;

public class TransactionZoneDbaseRecordsTranslatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<TransactionZoneDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly TransactionZoneDbaseRecordsTranslator _sut;

    public TransactionZoneDbaseRecordsTranslatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeOrganizationId();
        _fixture.Customize<TransactionZoneDbaseRecord>(
            composer => composer
                .FromFactory(random => new TransactionZoneDbaseRecord
                {
                    SOURCEID = { Value = random.Next(1, 9999) },
                    TYPE = { Value = random.Next(1, 9999) },
                    BESCHRIJV = { Value = _fixture.Create<Reason>().ToString() },
                    OPERATOR = { Value = _fixture.Create<OperatorName>().ToString() },
                    ORG = { Value = _fixture.Create<OrganizationId>().ToString() },
                    APPLICATIE =
                    {
                        Value = new string(_fixture
                            .CreateMany<char>(TransactionZoneDbaseRecord.Schema.APPLICATIE.Length.ToInt32())
                            .ToArray())
                    }
                })
                .OmitAutoProperties());

        _sut = new TransactionZoneDbaseRecordsTranslator();
        _enumerator = new List<TransactionZoneDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("transactiezone.dbf");
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsTranslator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<TransactionZoneDbaseRecord>>(_sut);
    }

    [Fact]
    public void TranslateEntryCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Translate(null, _enumerator, TranslatedChanges.Empty));
    }

    [Fact]
    public void TranslateRecordsCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, null, TranslatedChanges.Empty));
    }

    [Fact]
    public void TranslateChangesCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, _enumerator, null));
    }

    [Fact]
    public void TranslateWithoutRecordsReturnsExpectedResult()
    {
        var result = _sut.Translate(_entry, _enumerator, TranslatedChanges.Empty);

        Assert.Equal(
            TranslatedChanges.Empty,
            result);
    }

    [Fact]
    public void TranslateWithRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<TransactionZoneDbaseRecord>(1)
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();

        var result = _sut.Translate(_entry, enumerator, TranslatedChanges.Empty);

        var expected = records.Aggregate(
            TranslatedChanges.Empty,
            (changes, current) =>
                changes
                    .WithReason(new Reason(current.BESCHRIJV.Value))
                    .WithOperatorName(new OperatorName(current.OPERATOR.Value))
                    .WithOrganization(new OrganizationId(current.ORG.Value))
        );
        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }
}
