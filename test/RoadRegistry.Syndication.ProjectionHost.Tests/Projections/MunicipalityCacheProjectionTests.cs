namespace RoadRegistry.Syndication.ProjectionHost.Tests.Projections;

using AutoFixture;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Syndication.Projections;
using Syndication.Projections.MunicipalityEvents;

public class MunicipalityCacheProjectionTests
{
    public MunicipalityCacheProjectionTests()
    {
        _fixture = new Fixture();
    }

    private readonly Fixture _fixture;

    private MunicipalityBecameCurrent CreateMunicipalityBecameCurrent(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityBecameCurrent = _fixture.Create<MunicipalityBecameCurrent>();
        municipalityBecameCurrent.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        return municipalityBecameCurrent;
    }

    private MunicipalityWasRetired CreateMunicipalityBecameRetired(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityBecameRetired = _fixture.Create<MunicipalityWasRetired>();
        municipalityBecameRetired.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        return municipalityBecameRetired;
    }

    private MunicipalityNameWasCleared CreateMunicipalityNameWasCleared(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityNameWasCleared = _fixture.Create<MunicipalityNameWasCleared>();
        municipalityNameWasCleared.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        municipalityNameWasCleared.Language = MunicipalityLanguage.Dutch;
        return municipalityNameWasCleared;
    }

    private MunicipalityNameWasCorrected CreateMunicipalityNameWasCorrected(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityNameWasCorrected = _fixture.Create<MunicipalityNameWasCorrected>();
        municipalityNameWasCorrected.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        municipalityNameWasCorrected.Language = MunicipalityLanguage.Dutch;
        return municipalityNameWasCorrected;
    }

    private MunicipalityNameWasCorrectedToCleared CreateMunicipalityNameWasCorrectedToCleared(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityNameWasCorrectedToCleared = _fixture.Create<MunicipalityNameWasCorrectedToCleared>();
        municipalityNameWasCorrectedToCleared.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        municipalityNameWasCorrectedToCleared.Language = MunicipalityLanguage.Dutch;
        return municipalityNameWasCorrectedToCleared;
    }

    private MunicipalityNisCodeWasCorrected CreateMunicipalityNisCodeWasCorrected(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityNisCodeWasCorrected = _fixture.Create<MunicipalityNisCodeWasCorrected>();
        municipalityNisCodeWasCorrected.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        return municipalityNisCodeWasCorrected;
    }

    private MunicipalityNisCodeWasDefined CreateMunicipalityNisCodeWasDefined(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityNisCodeWasDefined = _fixture.Create<MunicipalityNisCodeWasDefined>();
        municipalityNisCodeWasDefined.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        return municipalityNisCodeWasDefined;
    }

    private MunicipalityWasCorrectedToCurrent CreateMunicipalityWasCorrectedToCurrent(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityWasCorrectedToCurrent = _fixture.Create<MunicipalityWasCorrectedToCurrent>();
        municipalityWasCorrectedToCurrent.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        return municipalityWasCorrectedToCurrent;
    }

    private MunicipalityWasCorrectedToRetired CreateMunicipalityWasCorrectedToRetired(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityWasCorrectedToRetired = _fixture.Create<MunicipalityWasCorrectedToRetired>();
        municipalityWasCorrectedToRetired.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        return municipalityWasCorrectedToRetired;
    }

    private MunicipalityWasNamed CreateMunicipalityWasNamed(MunicipalityWasRegistered municipalityWasRegistered)
    {
        var municipalityWasNamed = _fixture.Create<MunicipalityWasNamed>();
        municipalityWasNamed.MunicipalityId = municipalityWasRegistered.MunicipalityId;
        municipalityWasNamed.Language = MunicipalityLanguage.Dutch;
        return municipalityWasNamed;
    }

    [Fact]
    public Task When_clearing_municipality_names()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityWasNamed = CreateMunicipalityWasNamed(municipalityWasRegistered);
                var municipalityNameWasCleared = CreateMunicipalityNameWasCleared(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityWasNamed,
                        municipalityNameWasCleared
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_correcting_municipality_names()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityWasNamed = CreateMunicipalityWasNamed(municipalityWasRegistered);
                var municipalityNameWasCorrected = CreateMunicipalityNameWasCorrected(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = municipalityNameWasCorrected.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityWasNamed,
                        municipalityNameWasCorrected
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_correcting_municipality_names_to_cleared()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityWasNamed = CreateMunicipalityWasNamed(municipalityWasRegistered);
                var municipalityNameWasCorrectedToCleared = CreateMunicipalityNameWasCorrectedToCleared(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityWasNamed,
                        municipalityNameWasCorrectedToCleared
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_correcting_nis_codes()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityNisCodeWasDefined = CreateMunicipalityNisCodeWasDefined(municipalityWasRegistered);
                var municipalityNisCodeWasCorrected = CreateMunicipalityNisCodeWasCorrected(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = municipalityNisCodeWasCorrected.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityNisCodeWasDefined,
                        municipalityNisCodeWasCorrected
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_defining_nis_codes()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityNisCodeWasDefined = CreateMunicipalityNisCodeWasDefined(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = municipalityNisCodeWasDefined.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[] { municipalityWasRegistered, municipalityNisCodeWasDefined },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_municipality_became_current()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityBecameCurrent = CreateMunicipalityBecameCurrent(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Current,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityBecameCurrent
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_municipality_became_retired()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityBecameRetired = CreateMunicipalityBecameRetired(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Retired,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityBecameRetired
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_municipality_was_corrected__current()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityWasCorrectedToCurrent = CreateMunicipalityWasCorrectedToCurrent(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Current,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityWasCorrectedToCurrent
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_municipality_was_corrected__retired()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityWasCorrectedToRetired = CreateMunicipalityWasCorrectedToRetired(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Retired,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[]
                    {
                        municipalityWasRegistered,
                        municipalityWasCorrectedToRetired
                    },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_naming_municipalities()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(municipalityWasRegistered =>
            {
                var municipalityWasNamed = CreateMunicipalityWasNamed(municipalityWasRegistered);

                var expected = new MunicipalityRecord
                {
                    MunicipalityId = municipalityWasRegistered.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = municipalityWasRegistered.NisCode,
                    DutchName = municipalityWasNamed.Name,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    events = new object[] { municipalityWasRegistered, municipalityWasNamed },
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.SelectMany(d => d.events))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_registering_municipalities()
    {
        var data = _fixture
            .CreateMany<MunicipalityWasRegistered>()
            .Select(@event =>
            {
                var expected = new MunicipalityRecord
                {
                    MunicipalityId = @event.MunicipalityId,
                    MunicipalityStatus = MunicipalityStatus.Registered,
                    NisCode = @event.NisCode,
                    DutchName = null,
                    FrenchName = null,
                    GermanName = null,
                    EnglishName = null
                };

                return new
                {
                    @event,
                    expected
                };
            }).ToList();

        return new MunicipalityCacheProjection()
            .Scenario()
            .Given(data.Select(d => d.@event))
            .Expect(data.Select(d => d.expected));
    }
}
