namespace RoadRegistry.Syndication.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice.Core;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Syndication.Projections;
using Syndication.Projections.StreetNameEvents;

public class StreetNameCacheProjectionTests
{
    private readonly Fixture _fixture;

    public StreetNameCacheProjectionTests()
    {
        _fixture = new Fixture();
    }

    private StreetNameBecameCurrent CreateStreetNameBecameCurrent(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameBecameCurrent = _fixture.Create<StreetNameBecameCurrent>();
        streetNameBecameCurrent.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameBecameCurrent;
    }

    private StreetNameWasRetired CreateStreetNameBecameRetired(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasRetired = _fixture.Create<StreetNameWasRetired>();
        streetNameWasRetired.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameWasRetired;
    }

    private StreetNameHomonymAdditionWasCleared CreateStreetNameHomonymAdditionWasCleared(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameHomonymAdditionWasCleared = _fixture.Create<StreetNameHomonymAdditionWasCleared>();
        streetNameHomonymAdditionWasCleared.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameHomonymAdditionWasCleared.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameHomonymAdditionWasCleared;
    }

    private StreetNameHomonymAdditionWasCorrected CreateStreetNameHomonymAdditionWasCorrected(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasCorrected = _fixture.Create<StreetNameHomonymAdditionWasCorrected>();
        streetNameWasCorrected.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameWasCorrected.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameWasCorrected;
    }

    private StreetNameHomonymAdditionWasCorrectedToCleared CreateStreetNameHomonymAdditionWasCorrectedToCleared(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameHomonymAdditionWasCorrectedToCleared = _fixture.Create<StreetNameHomonymAdditionWasCorrectedToCleared>();
        streetNameHomonymAdditionWasCorrectedToCleared.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameHomonymAdditionWasCorrectedToCleared.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameHomonymAdditionWasCorrectedToCleared;
    }

    private StreetNameHomonymAdditionWasDefined CreateStreetNameHomonymAdditionWasDefined(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasNamed = _fixture.Create<StreetNameHomonymAdditionWasDefined>();
        streetNameWasNamed.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameWasNamed.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameWasNamed;
    }

    private StreetNameNameWasCleared CreateStreetNameNameWasCleared(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameNameWasCleared = _fixture.Create<StreetNameNameWasCleared>();
        streetNameNameWasCleared.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameNameWasCleared.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameNameWasCleared;
    }

    private StreetNameNameWasCorrected CreateStreetNameNameWasCorrected(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameNameWasCorrected = _fixture.Create<StreetNameNameWasCorrected>();
        streetNameNameWasCorrected.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameNameWasCorrected.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameNameWasCorrected;
    }

    private StreetNameNameWasCorrectedToCleared CreateStreetNameNameWasCorrectedToCleared(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameNameWasCorrectedToCleared = _fixture.Create<StreetNameNameWasCorrectedToCleared>();
        streetNameNameWasCorrectedToCleared.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameNameWasCorrectedToCleared.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameNameWasCorrectedToCleared;
    }

    private StreetNamePersistentLocalIdentifierWasAssigned CreateStreetNamePersistentLocalIdWasAssigned(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNamePersistentLocalIdWasAssigned = _fixture.Create<StreetNamePersistentLocalIdentifierWasAssigned>();
        streetNamePersistentLocalIdWasAssigned.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNamePersistentLocalIdWasAssigned.PersistentLocalId = _fixture.Create<int>();
        return streetNamePersistentLocalIdWasAssigned;
    }

    private StreetNameStatusWasCorrectedToRemoved CreateStreetNameStatusWasCorrectedToRemoved(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameStatusWasCorrectedToRemoved = _fixture.Create<StreetNameStatusWasCorrectedToRemoved>();
        streetNameStatusWasCorrectedToRemoved.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameStatusWasCorrectedToRemoved;
    }

    private StreetNameStatusWasRemoved CreateStreetNameStatusWasRemoved(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameStatusWasRemoved = _fixture.Create<StreetNameStatusWasRemoved>();
        streetNameStatusWasRemoved.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameStatusWasRemoved;
    }

    private StreetNameWasCorrectedToCurrent CreateStreetNameWasCorrectedToCurrent(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasCorrectedToCurrent = _fixture.Create<StreetNameWasCorrectedToCurrent>();
        streetNameWasCorrectedToCurrent.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameWasCorrectedToCurrent;
    }

    private StreetNameWasCorrectedToProposed CreateStreetNameWasCorrectedToProposed(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasCorrectedToProposed = _fixture.Create<StreetNameWasCorrectedToProposed>();
        streetNameWasCorrectedToProposed.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameWasCorrectedToProposed;
    }

    private StreetNameWasCorrectedToRetired CreateStreetNameWasCorrectedToRetired(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasCorrectedToRetired = _fixture.Create<StreetNameWasCorrectedToRetired>();
        streetNameWasCorrectedToRetired.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameWasCorrectedToRetired;
    }

    private StreetNameWasNamed CreateStreetNameWasNamed(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasNamed = _fixture.Create<StreetNameWasNamed>();
        streetNameWasNamed.StreetNameId = streetNameWasRegistered.StreetNameId;
        streetNameWasNamed.LanguageValue = StreetNameLanguage.Dutch.GetDisplayName();
        return streetNameWasNamed;
    }

    private StreetNameWasProposed CreateStreetNameWasProposed(StreetNameWasRegistered streetNameWasRegistered)
    {
        var streetNameWasProposed = _fixture.Create<StreetNameWasProposed>();
        streetNameWasProposed.StreetNameId = streetNameWasRegistered.StreetNameId;
        return streetNameWasProposed;
    }

    [Fact]
    public Task When_street_name_became_current()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameBecameCurrent = CreateStreetNameBecameCurrent(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameBecameCurrent
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = StreetNameStatus.Current,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_became_proposed()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameBecameProposed = CreateStreetNameWasProposed(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameBecameProposed
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = StreetNameStatus.Proposed,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_became_retired()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameBecameRetired = CreateStreetNameBecameRetired(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameBecameRetired
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = StreetNameStatus.Retired,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_homonym_addition_was_cleared()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var homonymAdditionWasDefined = CreateStreetNameHomonymAdditionWasDefined(streetNameWasRegistered);
                var homonymAdditionWasCleared = CreateStreetNameHomonymAdditionWasCleared(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    homonymAdditionWasDefined,
                    homonymAdditionWasCleared
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = streetNameWasNamed.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_homonym_addition_was_corrected()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var homonymAdditionWasDefined = CreateStreetNameHomonymAdditionWasDefined(streetNameWasRegistered);
                var homonymAdditionWasCorrected = CreateStreetNameHomonymAdditionWasCorrected(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    homonymAdditionWasDefined,
                    homonymAdditionWasCorrected
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = streetNameWasNamed.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = homonymAdditionWasCorrected.HomonymAddition,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_homonym_addition_was_corrected_to_cleared()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var homonymAdditionWasDefined = CreateStreetNameHomonymAdditionWasDefined(streetNameWasRegistered);
                var homonymAdditionWasCorrectedToCleared = CreateStreetNameHomonymAdditionWasCorrectedToCleared(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    homonymAdditionWasDefined,
                    homonymAdditionWasCorrectedToCleared
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = streetNameWasNamed.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_homonym_addition_was_defined()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var homonymAdditionWasDefined = CreateStreetNameHomonymAdditionWasDefined(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    homonymAdditionWasDefined
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = streetNameWasNamed.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = homonymAdditionWasDefined.HomonymAddition,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_name_was_cleared()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var streetNameNameWasCleared = CreateStreetNameNameWasCleared(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    streetNameNameWasCleared
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_name_was_corrected()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var streetNameNameWasCorrected = CreateStreetNameNameWasCorrected(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    streetNameNameWasCorrected
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = streetNameNameWasCorrected.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_status_was_corrected_to_removed()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameBecameCurrent = CreateStreetNameBecameCurrent(streetNameWasRegistered);
                var streetNameStatusWasCorrectedToRemoved = CreateStreetNameStatusWasCorrectedToRemoved(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameBecameCurrent,
                    streetNameStatusWasCorrectedToRemoved
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_status_was_removed()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameBecameCurrent = CreateStreetNameBecameCurrent(streetNameWasRegistered);
                var streetNameStatusWasRemoved = CreateStreetNameStatusWasRemoved(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameBecameCurrent,
                    streetNameStatusWasRemoved
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_assigned_a_persistent_local_id()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNamePersistentLocalIdWasAssigned = CreateStreetNamePersistentLocalIdWasAssigned(streetNameWasRegistered);

                var events = new object[] { streetNameWasRegistered, streetNamePersistentLocalIdWasAssigned };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = streetNamePersistentLocalIdWasAssigned.PersistentLocalId,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_corrected_to_cleared()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);
                var streetNameNameWasCorrectedToCleared = CreateStreetNameNameWasCorrectedToCleared(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasNamed,
                    streetNameNameWasCorrectedToCleared
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_corrected_to_current()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasCorrectedToCurrent = CreateStreetNameWasCorrectedToCurrent(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasCorrectedToCurrent
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = StreetNameStatus.Current,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_corrected_to_proposed()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasCorrectedToProposed = CreateStreetNameWasCorrectedToProposed(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasCorrectedToProposed
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = StreetNameStatus.Proposed,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_corrected_to_retired()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasCorrectedToRetired = CreateStreetNameWasCorrectedToRetired(streetNameWasRegistered);

                var events = new object[]
                {
                    streetNameWasRegistered,
                    streetNameWasCorrectedToRetired
                };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = StreetNameStatus.Retired,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_named()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var streetNameWasNamed = CreateStreetNameWasNamed(streetNameWasRegistered);

                var events = new object[] { streetNameWasRegistered, streetNameWasNamed };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = streetNameWasNamed.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_street_name_was_registered()
    {
        var data = _fixture
            .CreateMany<StreetNameWasRegistered>()
            .Select((streetNameWasRegistered, counter) =>
            {
                var events = new object[] { streetNameWasRegistered };

                var expected = new StreetNameRecord
                {
                    StreetNameId = streetNameWasRegistered.StreetNameId,
                    PersistentLocalId = null,
                    MunicipalityId = streetNameWasRegistered.MunicipalityId,
                    NisCode = streetNameWasRegistered.NisCode,
                    Name = null,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null,
                    StreetNameStatus = null,
                    HomonymAddition = null,
                    DutchHomonymAddition = null,
                    FrenchHomonymAddition = null,
                    GermanHomonymAddition = null,
                    EnglishHomonymAddition = null,
                    Position = counter * events.Length + events.Length - 1
                };

                return new
                {
                    events,
                    expected
                };
            }).ToList();

        return new StreetNameCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }
}