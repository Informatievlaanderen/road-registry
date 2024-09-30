namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.Organizations.V2;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extensions;
using Editor.Schema.Organizations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
//TODO-rik update tests
public class OrganizationRecordV2ProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public OrganizationRecordV2ProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

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

        var expected = new OrganizationRecordV2
        {
            Id = 1,
            Code = createOrganizationAccepted.Code,
            SortableCode = OrganizationRecordV2Projection.GetSortableCodeFor(createOrganizationAccepted.Code),
            DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion,
            DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = createOrganizationAccepted.Code },
                LBLORG = { Value = createOrganizationAccepted.Name },
                OVOCODE = { Value = createOrganizationAccepted.OvoCode }
            }.ToBytes(_services.MemoryStreamManager, _services.FileEncoding)
        };

        return new OrganizationRecordV2Projection(_services.MemoryStreamManager, _services.FileEncoding, new NullLogger<OrganizationRecordV2Projection>())
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

        return new OrganizationRecordV2Projection(_services.MemoryStreamManager, _services.FileEncoding, new NullLogger<OrganizationRecordV2Projection>())
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
            SortableCode = OrganizationRecordV2Projection.GetSortableCodeFor(importedOrganization.Code),
            DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion,
            DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = importedOrganization.Code },
                LBLORG = { Value = renameOrganizationAccepted.Name }
            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
        };

        return new OrganizationRecordV2Projection(_services.MemoryStreamManager, _services.FileEncoding, new NullLogger<OrganizationRecordV2Projection>())
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
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };

        var expected = new OrganizationRecordV2
        {
            Id = 1,
            Code = importedOrganization.Code,
            SortableCode = OrganizationRecordV2Projection.GetSortableCodeFor(importedOrganization.Code),
            DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion,
            DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = importedOrganization.Code },
                LBLORG = { Value = importedOrganization.Name },
                OVOCODE = { Value = changeOrganizationAccepted.OvoCode }
            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
        };

        return new OrganizationRecordV2Projection(_services.MemoryStreamManager, _services.FileEncoding, new NullLogger<OrganizationRecordV2Projection>())
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
                    SortableCode = OrganizationRecordV2Projection.GetSortableCodeFor(@event.Code),
                    DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion,
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

        return new OrganizationRecordV2Projection(_services.MemoryStreamManager, _services.FileEncoding, new NullLogger<OrganizationRecordV2Projection>())
            .Scenario()
            .Given(data.Select(d => d.ImportedOrganization))
            .Expect(data.Select(d => d.Expected));
    }
}
