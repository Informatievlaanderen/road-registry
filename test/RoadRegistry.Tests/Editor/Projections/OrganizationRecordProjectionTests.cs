namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Framework.Projections;
    using Microsoft.IO;
    using Schema.Organizations;
    using Xunit;

    public class OrganizationRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public OrganizationRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.Customize<ImportedOrganization>(
                customization =>
                    customization.FromFactory(_ =>
                        new ImportedOrganization
                        {
                            Code = _fixture.Create<OrganizationId>(),
                            Name = _fixture.Create<OrganizationName>()
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
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    };
                    return new
                    {
                        ImportedOrganization = @event,
                        Expected = expected
                    };
                }).ToList();

            return new OrganizationRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedOrganization))
                .Expect(data.Select(d => d.Expected));
        }
    }
}
