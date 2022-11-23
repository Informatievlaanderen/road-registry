namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Organizations;
using Microsoft.IO;
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

    [Fact]
    public Task When_organization_is_renamed()
    {
        var messages = new List<object>
        {
            new ImportedOrganization { Code = "ABC", Name = "Organization Inc." },
            new RenameOrganizationAccepted
            {
                Code = "ABC",
                Name = "Alphabet"
            }
        };

        var expected = new OrganizationRecord
        {
            Id = 1,
            Code = "ABC",
            SortableCode = OrganizationRecordProjection.GetSortableCodeFor("ABC"),
            DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = "ABC" },
                LBLORG = { Value = "Alphabet" }
            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
        };

        return new OrganizationRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
            .Scenario()
            .Given(messages)
            .Expect(new object[] { expected });
    }
}
