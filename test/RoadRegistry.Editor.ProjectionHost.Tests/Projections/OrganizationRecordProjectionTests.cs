namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using Editor.Projections;
using Microsoft.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Editor.Schema.Organizations;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class OrganizationRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

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
