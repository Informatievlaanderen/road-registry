namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Messages;
using Editor.Projections;
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
        var importedOrganization = new ImportedOrganization { Code = "ABC", Name = "Organization Inc." };
        var renameOrganizationAccepted = new RenameOrganizationAccepted
        {
            Code = "ABC",
            Name = "Alphabet"
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
            .Given(importedOrganization, renameOrganizationAccepted)
            .Expect(expected);
    }

    [Fact]
    public Task When_organization_is_created()
    {
        var createOrganizationAccepted = new CreateOrganizationAccepted { Code = "ABC", Name = "Alphabet" };

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
            .Given(createOrganizationAccepted)
            .Expect(expected);
    }

    [Fact]
    public Task When_organization_is_deleted()
    {
        var createOrganizationAccepted = new CreateOrganizationAccepted { Code = "ABC", Name = "Alphabet" };
        var deleteOrganizationAccepted = new DeleteOrganizationAccepted { Code = "ABC" };
        
        return new OrganizationRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
            .Scenario()
            .Given(createOrganizationAccepted, deleteOrganizationAccepted)
            .Expect(Enumerable.Empty<OrganizationRecord>());
    }
}
