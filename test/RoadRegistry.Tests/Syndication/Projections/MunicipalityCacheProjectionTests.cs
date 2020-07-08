namespace RoadRegistry.Syndication.Projections
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Framework.Projections;
    using MunicipalityEvents;
    using Schema;
    using Xunit;

    public class MunicipalityCacheProjectionTests
    {
        private readonly Fixture _fixture;

        public MunicipalityCacheProjectionTests()
        {
            _fixture = new Fixture();
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
                        EnglishName = null,
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

        [Fact]
        public Task When_naming_municipalities()
        {
            var data = _fixture
                .CreateMany<MunicipalityWasRegistered>()
                .Select(municipalityWasRegistered =>
                {
                    var municipalityWasNamed = _fixture.Create<MunicipalityWasNamed>();
                    municipalityWasNamed.MunicipalityId = municipalityWasRegistered.MunicipalityId;
                    municipalityWasNamed.Language = Language.Dutch;

                    var expected = new MunicipalityRecord
                    {
                        MunicipalityId = municipalityWasRegistered.MunicipalityId,
                        MunicipalityStatus = MunicipalityStatus.Registered,
                        NisCode = municipalityWasRegistered.NisCode,
                        DutchName = municipalityWasNamed.Name,
                        FrenchName = null,
                        GermanName = null,
                        EnglishName = null,
                    };

                    return new
                    {
                        events = new object[]{ municipalityWasRegistered, municipalityWasNamed},
                        expected
                    };
                }).ToList();

            return new MunicipalityCacheProjection()
                .Scenario()
                .Given(data.SelectMany(d => d.events))
                .Expect(data.Select(d => d.expected));
        }
    }
}
