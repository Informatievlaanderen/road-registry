namespace RoadRegistry.Projections.Tests
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Xunit;

    public class OrganizationTests
    {
        private readonly ScenarioFixture _fixture;

        public OrganizationTests()
        {
            _fixture = new ScenarioFixture();
        }

        [Fact]
        public Task When_organizations_are_imported()
        {
            var data = _fixture
                .CreateMany<ImportedOrganization>()
                .Select((organization, i) =>
                {
                    var expectedGeneratedId = i + 1;
                    var expected = new OrganizationRecord
                    {
                        Id = expectedGeneratedId,
                        Code = organization.Code,
                        SortableCode = OrganizationRecordProjection.GetSortableCodeFor(organization.Code),
                        DbaseRecord = new OrganizationDbaseRecord
                        {
                            ORG = { Value = organization.Code },
                            LBLORG = { Value = organization.Name },
                        }.ToBytes(Encoding.UTF8),
                    };
                    return new
                    {
                        ImportedOrganization = organization,
                        Expected = expected,
                    };
                }).ToList();

            return new OrganizationRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedOrganization))
                .Expect(data.Select(d => d.Expected));
        }
    }
}
