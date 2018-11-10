namespace RoadRegistry.Projections.Tests
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Messages;
    using Xunit;

    public class GradeSeparatedJunctionRecordProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly GradeSeparatedJunctionTypeTranslator _gradeSeparatedJunctionTypeTranslator;

        public GradeSeparatedJunctionRecordProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _gradeSeparatedJunctionTypeTranslator = new GradeSeparatedJunctionTypeTranslator();
        }

        [Fact]
        public Task When_importing_grade_separated_junctions()
        {
            var data = _fixture
                .CreateMany<ImportedGradeSeparatedJunction>()
                .Select(junction =>
                {
                    var expected = new GradeSeparatedJunctionRecord
                    {
                        Id = junction.Id,
                        DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                        {
                            OK_OIDN = { Value = junction.Id },
                            TYPE = { Value = _gradeSeparatedJunctionTypeTranslator.TranslateToIdentifier(junction.Type) },
                            LBLTYPE = { Value = _gradeSeparatedJunctionTypeTranslator.TranslateToDutchName(junction.Type) },
                            BO_WS_OIDN = { Value = junction.UpperRoadSegmentId },
                            ON_WS_OIDN = { Value = junction.LowerRoadSegmentId },
                            BEGINTIJD = { Value = junction.Origin.Since },
                            BEGINORG = { Value = junction.Origin.OrganizationId },
                            LBLBGNORG = { Value = junction.Origin.Organization }
                        }.ToBytes(Encoding.UTF8)
                    };

                    return new
                    {
                        junction,
                        expected
                    };
                }).ToList();

            return new GradeSeparatedJunctionRecordProjection(_gradeSeparatedJunctionTypeTranslator, Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.junction))
                .Expect(data.Select(d => d.expected));
        }
    }
}
