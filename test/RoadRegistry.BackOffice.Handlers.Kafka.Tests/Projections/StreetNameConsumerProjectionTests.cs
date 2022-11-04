#nullable enable

namespace RoadRegistry.BackOffice.Handlers.Kafka.Tests.Projections;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Fixtures;
using StreetNameConsumer.Schema;

public class StreetNameConsumerProjectionTests : IClassFixture<StreetNameConsumerProjectionFixture>
{
    private readonly StreetNameConsumerProjectionFixture _fixture;

    public StreetNameConsumerProjectionTests(StreetNameConsumerProjectionFixture fixture)
    {
        _fixture = fixture;
    }

    private StreetNameConsumerItem? Current => _fixture.GetConsumerItem(_fixture.StreetNameIdDefault);

    private StreetNameConsumerItem? Item(string streetNameId)
    {
        return _fixture.GetConsumerItem(streetNameId);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameBecameCurrent()
    {
        await _fixture.ProjectAsync(new StreetNameBecameCurrent(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Equal(StreetNameStatus.Current, Current?.StreetNameStatus);
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameHomonymAdditionWasCleared(StreetNameLanguage? language)
    {
        await _fixture.ProjectAsync(new StreetNameHomonymAdditionWasCleared(
            _fixture.StreetNameIdDefault,
            language?.ToString(),
            _fixture.Provenance
        ));

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Null(currentItem.DutchHomonymAddition);
                break;
            case StreetNameLanguage.French:
                Assert.Null(currentItem.FrenchHomonymAddition);
                break;
            case StreetNameLanguage.German:
                Assert.Null(currentItem.GermanHomonymAddition);
                break;
            case StreetNameLanguage.English:
                Assert.Null(currentItem.EnglishHomonymAddition);
                break;
            default:
                Assert.Null(currentItem.HomonymAddition);
                break;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameHomonymAdditionWasCorrected(StreetNameLanguage? language)
    {
        var homonymAddition = language?.ToString() ?? "";

        await _fixture.ProjectAsync(new StreetNameHomonymAdditionWasCorrected(
            _fixture.StreetNameIdDefault,
            homonymAddition,
            language?.ToString(),
            _fixture.Provenance
        ));

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Equal(homonymAddition, currentItem.DutchHomonymAddition);
                break;
            case StreetNameLanguage.French:
                Assert.Equal(homonymAddition, currentItem.FrenchHomonymAddition);
                break;
            case StreetNameLanguage.German:
                Assert.Equal(homonymAddition, currentItem.GermanHomonymAddition);
                break;
            case StreetNameLanguage.English:
                Assert.Equal(homonymAddition, currentItem.EnglishHomonymAddition);
                break;
            case null:
                Assert.Equal(homonymAddition, currentItem.HomonymAddition);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameHomonymAdditionWasCorrectedToCleared(StreetNameLanguage? language)
    {
        await _fixture.ProjectAsync(new StreetNameHomonymAdditionWasCorrectedToCleared(
            _fixture.StreetNameIdDefault,
            language?.ToString(),
            _fixture.Provenance
        ));

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Null(currentItem.DutchHomonymAddition);
                break;
            case StreetNameLanguage.French:
                Assert.Null(currentItem.FrenchHomonymAddition);
                break;
            case StreetNameLanguage.German:
                Assert.Null(currentItem.GermanHomonymAddition);
                break;
            case StreetNameLanguage.English:
                Assert.Null(currentItem.EnglishHomonymAddition);
                break;
            case null:
                Assert.Null(currentItem.HomonymAddition);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameHomonymAdditionWasDefined(StreetNameLanguage? language)
    {
        var homonymAddition = language?.ToString() ?? "";

        await _fixture.ProjectAsync(new StreetNameHomonymAdditionWasDefined(
            _fixture.StreetNameIdDefault,
            homonymAddition,
            language?.ToString(),
            _fixture.Provenance
        ));

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");

        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Equal(homonymAddition, currentItem.DutchHomonymAddition);
                break;
            case StreetNameLanguage.French:
                Assert.Equal(homonymAddition, currentItem.FrenchHomonymAddition);
                break;
            case StreetNameLanguage.German:
                Assert.Equal(homonymAddition, currentItem.GermanHomonymAddition);
                break;
            case StreetNameLanguage.English:
                Assert.Equal(homonymAddition, currentItem.EnglishHomonymAddition);
                break;
            case null:
                Assert.Equal(homonymAddition, currentItem.HomonymAddition);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameNameWasCleared(StreetNameLanguage? language)
    {
        await _fixture.ProjectAsync(new StreetNameNameWasCleared(
            _fixture.StreetNameIdDefault,
            language?.ToString(),
            _fixture.Provenance
        ));

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Null(currentItem.DutchName);
                break;
            case StreetNameLanguage.French:
                Assert.Null(currentItem.FrenchName);
                break;
            case StreetNameLanguage.German:
                Assert.Null(currentItem.GermanName);
                break;
            case StreetNameLanguage.English:
                Assert.Null(currentItem.EnglishName);
                break;
            case null:
                Assert.Null(currentItem.Name);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameNameWasCorrected(StreetNameLanguage? language)
    {
        await _fixture.ProjectAsync(new StreetNameNameWasCorrected(
            _fixture.StreetNameIdDefault,
            "Testlaan",
            language?.ToString(),
            _fixture.Provenance
        ));

        const string expected = "Testlaan";

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Equal(expected, currentItem.DutchName);
                break;
            case StreetNameLanguage.French:
                Assert.Equal(expected, currentItem.FrenchName);
                break;
            case StreetNameLanguage.German:
                Assert.Equal(expected, currentItem.GermanName);
                break;
            case StreetNameLanguage.English:
                Assert.Equal(expected, currentItem.EnglishName);
                break;
            case null:
                Assert.Equal(expected, currentItem.Name);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameNameWasCorrectedToCleared(StreetNameLanguage? language)
    {
        await _fixture.ProjectAsync(new StreetNameNameWasCorrectedToCleared(
            _fixture.StreetNameIdDefault,
            language.ToString(),
            _fixture.Provenance
        ));

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Null(currentItem.DutchName);
                break;
            case StreetNameLanguage.French:
                Assert.Null(currentItem.FrenchName);
                break;
            case StreetNameLanguage.German:
                Assert.Null(currentItem.GermanName);
                break;
            case StreetNameLanguage.English:
                Assert.Null(currentItem.EnglishName);
                break;
            case null:
                Assert.Null(currentItem.Name);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Fact]
    public async Task ItShouldProject_StreetNameStatusWasCorrectedToRemoved()
    {
        await _fixture.ProjectAsync(new StreetNameStatusWasCorrectedToRemoved(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Null(Current.StreetNameStatus);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameStatusWasRemoved()
    {
        await _fixture.ProjectAsync(new StreetNameStatusWasRemoved(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Null(Current.StreetNameStatus);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameWasCorrectedToCurrent()
    {
        await _fixture.ProjectAsync(new StreetNameWasCorrectedToCurrent(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Equal(StreetNameStatus.Current, Current?.StreetNameStatus);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameWasCorrectedToProposed()
    {
        await _fixture.ProjectAsync(new StreetNameWasCorrectedToProposed(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Equal(StreetNameStatus.Proposed, Current?.StreetNameStatus);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameWasCorrectedToRetired()
    {
        await _fixture.ProjectAsync(new StreetNameWasCorrectedToRetired(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Equal(StreetNameStatus.Retired, Current?.StreetNameStatus);
    }

    [Theory]
    [MemberData(nameof(ValidLanguageCases))]
    public async Task ItShouldProject_StreetNameWasNamed(StreetNameLanguage? language)
    {
        await _fixture.ProjectAsync(new StreetNameWasNamed(
            _fixture.StreetNameIdDefault,
            "Teststraat",
            language?.ToString(),
            _fixture.Provenance
        ));

        const string expected = "Teststraat";

        var currentItem = Current ?? throw new NullReferenceException("Could not find current item");
        switch (language)
        {
            case StreetNameLanguage.Dutch:
                Assert.Equal(expected, currentItem.DutchName);
                break;
            case StreetNameLanguage.French:
                Assert.Equal(expected, currentItem.FrenchName);
                break;
            case StreetNameLanguage.German:
                Assert.Equal(expected, currentItem.GermanName);
                break;
            case StreetNameLanguage.English:
                Assert.Equal(expected, currentItem.EnglishName);
                break;
            case null:
                Assert.Equal(expected, currentItem.Name);
                break;
            default:
                Assert.Fail("Unknown StreetNameLanguage");
                break;
        }
    }

    [Fact]
    public async Task ItShouldProject_StreetNameWasProposed()
    {
        await _fixture.ProjectAsync(new StreetNameWasProposed(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Equal(StreetNameStatus.Proposed, Current?.StreetNameStatus);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameWasRegistered()
    {
        var nisCode = "12345";
        var streetNameId = "2DC71FB62F6746FBBDC02C9583E8BDB1";

        await _fixture.ProjectAsync(new StreetNameWasRegistered(
            streetNameId,
            _fixture.MunicipalityIdDefault,
            nisCode,
            _fixture.Provenance
        ));

        var currentItem = Item(streetNameId) ?? throw new NullReferenceException("Could not find current item");

        Assert.Equal(streetNameId, currentItem.StreetNameId);
        Assert.Null(currentItem.PersistentLocalId);
        Assert.Equal(_fixture.MunicipalityIdDefault, currentItem.MunicipalityId);
        Assert.Equal(_fixture.MunicipalityIdDefault, currentItem.MunicipalityId);
        Assert.Null(currentItem.Name);
        Assert.Null(currentItem.DutchName);
        Assert.Null(currentItem.FrenchName);
        Assert.Null(currentItem.GermanName);
        Assert.Null(currentItem.EnglishName);
        Assert.Null(currentItem.StreetNameStatus);
    }

    [Fact]
    public async Task ItShouldProject_StreetNameWasRetired()
    {
        await _fixture.ProjectAsync(new StreetNameWasRetired(
            _fixture.StreetNameIdDefault,
            _fixture.Provenance
        ));

        Assert.Equal(StreetNameStatus.Retired, Current?.StreetNameStatus);
    }

    public static IEnumerable<object[]> ValidLanguageCases()
    {
        yield return new object[] { StreetNameLanguage.Dutch };
        yield return new object[] { StreetNameLanguage.French };
        yield return new object[] { StreetNameLanguage.German };
        yield return new object[] { StreetNameLanguage.English };
        yield return new object[] { null };
    }
}
