namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.Organizations;

public class OrganizationLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public OrganizationLatestItemProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeOrganizationOvoCode();

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
    public Task When_organization_is_created()
    {
        var createOrganizationAccepted = new CreateOrganizationAccepted {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>(),
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };

        var expected = new OrganizationLatestItem
        {
            Code = createOrganizationAccepted.Code,
            SortableCode = OrganizationLatestItemProjection.GetSortableCodeFor(createOrganizationAccepted.Code),
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            IsRemoved = false
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(createOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_deleted()
    {
        var createOrganizationAccepted = new CreateOrganizationAccepted
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>()
        };
        var deleteOrganizationAccepted = new DeleteOrganizationAccepted
        {
            Code = createOrganizationAccepted.Code
        };

        var expected = new OrganizationLatestItem
        {
            Code = createOrganizationAccepted.Code,
            SortableCode = OrganizationLatestItemProjection.GetSortableCodeFor(createOrganizationAccepted.Code),
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            IsRemoved = true
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(createOrganizationAccepted, deleteOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_renamed()
    {
        var importedOrganization = new ImportedOrganization
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>()
        };
        var renameOrganizationAccepted = new RenameOrganizationAccepted
        {
            Code = importedOrganization.Code,
            Name = _fixture.CreateWhichIsDifferentThan(new OrganizationName(importedOrganization.Name))
        };

        var expected = new OrganizationLatestItem
        {
            Code = importedOrganization.Code,
            SortableCode = OrganizationLatestItemProjection.GetSortableCodeFor(importedOrganization.Code),
            Name = renameOrganizationAccepted.Name,
            OvoCode = null,
            IsRemoved = false
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(importedOrganization, renameOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_changed()
    {
        var importedOrganization = new ImportedOrganization
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>()
        };
        var changeOrganizationAccepted = new ChangeOrganizationAccepted
        {
            Code = importedOrganization.Code,
            Name = importedOrganization.Name,
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };

        var expected = new OrganizationLatestItem
        {
            Code = importedOrganization.Code,
            SortableCode = OrganizationLatestItemProjection.GetSortableCodeFor(importedOrganization.Code),
            Name = changeOrganizationAccepted.Name,
            OvoCode = changeOrganizationAccepted.OvoCode,
            IsRemoved = false
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(importedOrganization, changeOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organizations_are_imported()
    {
        var data = _fixture
            .CreateMany<ImportedOrganization>(new Random().Next(1, 100))
            .Select((@event, _) =>
            {
                var expected = new OrganizationLatestItem
                {
                    Code = @event.Code,
                    SortableCode = OrganizationLatestItemProjection.GetSortableCodeFor(@event.Code),
                    Name = @event.Name,
                    OvoCode = null,
                    IsRemoved = false
                };
                return new
                {
                    ImportedOrganization = @event,
                    Expected = expected
                };
            }).ToList();

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.ImportedOrganization))
            .Expect(data.Select(d => d.Expected));
    }
}
