namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using Framework.Testing.Projections;
    using Messages;
    using Model;
    using Schema.Organizations;
    using Xunit;

    public class OrganizationTests
    {
        private readonly Fixture _fixture;

        public OrganizationTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
            _fixture.Customize<ImportedOrganization>(
                customization =>
                    customization.FromFactory(_ =>
                        new ImportedOrganization
                        {
                            Code = _fixture.Create<MaintenanceAuthorityId>(),
                            Name = _fixture.Create<MaintenanceAuthorityName>()
                        }
                    ).OmitAutoProperties()
            );
        }

        [Fact]
        public Task When_organizations_are_imported()
        {
            var data = _fixture
                .CreateMany<ImportedOrganization>(new Random().Next(1, 100))
                .Select((@event, i) =>
                {
                    var expected = new OrganizationRecord
                    {
                        Id = i + 1,
                        Code = @event.Code,
                        SortableCode = OrganizationRecordProjection.GetSortableCodeFor(@event.Code),
                        DbaseRecord = new OrganizationDbaseRecord
                        {
                            ORG = { Value = @event.Code },
                            LBLORG = { Value = @event.Name }
                        }.ToBytes(Encoding.UTF8)
                    };
                    return new
                    {
                        ImportedOrganization = @event,
                        Expected = expected
                    };
                }).ToList();

            return new OrganizationRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedOrganization))
                .Expect(data.Select(d => d.Expected));
        }
    }
}
