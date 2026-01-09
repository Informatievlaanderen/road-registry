namespace RoadRegistry.Product.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Product.Projections;
using Product.Schema.Organizations;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class OrganizationRecordV2ProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;

    public OrganizationRecordV2ProjectionTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeOrganizationOvoCode();
        _fixture.CustomizeOrganizationKboNumber();
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
            OvoCode = _fixture.Create<OrganizationOvoCode>(),
            KboNumber = _fixture.Create<OrganizationKboNumber>()
        };

        var expected = new OrganizationRecordV2
        {
            Id = 1,
            Code = createOrganizationAccepted.Code,
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            KboNumber = createOrganizationAccepted.KboNumber
        };

        return new OrganizationRecordV2Projection()
            .Scenario()
            .Given(createOrganizationAccepted)
            .Expect(expected);
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

        return new OrganizationRecordV2Projection()
            .Scenario()
            .Given(createOrganizationAccepted, deleteOrganizationAccepted)
            .Expect(Enumerable.Empty<OrganizationRecordV2>());
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

        var expected = new OrganizationRecordV2
        {
            Id = 1,
            Code = importedOrganization.Code,
            Name = renameOrganizationAccepted.Name
        };

        return new OrganizationRecordV2Projection()
            .Scenario()
            .Given(importedOrganization, renameOrganizationAccepted)
            .Expect(expected);
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
            OvoCode = _fixture.Create<OrganizationOvoCode>(),
            KboNumber = _fixture.Create<OrganizationKboNumber>(),
            IsMaintainer = _fixture.Create<bool>()
        };

        var expected = new OrganizationRecordV2
        {
            Id = 1,
            Code = importedOrganization.Code,
            Name = importedOrganization.Name,
            OvoCode = changeOrganizationAccepted.OvoCode,
            KboNumber = changeOrganizationAccepted.KboNumber,
            IsMaintainer = changeOrganizationAccepted.IsMaintainer
        };

        return new OrganizationRecordV2Projection()
            .Scenario()
            .Given(importedOrganization, changeOrganizationAccepted)
            .Expect(expected);
    }

    [Fact]
    public Task When_organizations_are_imported()
    {
        var data = _fixture
            .CreateMany<ImportedOrganization>(new Random().Next(1, 100))
            .Select((@event, i) =>
            {
                var expected = new OrganizationRecordV2
                {
                    Id = i + 1,
                    Code = @event.Code,
                    Name = @event.Name
                };
                return new
                {
                    ImportedOrganization = @event,
                    Expected = expected
                };
            }).ToList();

        return new OrganizationRecordV2Projection()
            .Scenario()
            .Given(data.Select(d => d.ImportedOrganization))
            .Expect(data.Select(d => d.Expected));
    }
}
