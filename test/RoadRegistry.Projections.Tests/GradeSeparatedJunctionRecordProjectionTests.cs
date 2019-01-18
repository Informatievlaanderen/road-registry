namespace RoadRegistry.Projections.Tests
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOfficeSchema.GradeSeparatedJunctions;
    using Messages;
    using Model;
    using Xunit;

    public class GradeSeparatedJunctionRecordProjectionTests
    {
        private readonly Fixture _fixture;

        public GradeSeparatedJunctionRecordProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeGradeSeparatedJunctionType();
            _fixture.CustomizeGradeSeparatedJunctionId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
            _fixture.CustomizeOriginProperties();
            _fixture.CustomizeImportedGradeSeparatedJunction();
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
                            TYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier },
                            LBLTYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name },
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

            return new GradeSeparatedJunctionRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.junction))
                .Expect(data.Select(d => d.expected));
        }
    }
}
