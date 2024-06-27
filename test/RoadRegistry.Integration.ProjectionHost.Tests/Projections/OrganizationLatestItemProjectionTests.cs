namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using System.Globalization;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.Tests.BackOffice;
using Schema.Organizations;

public class OrganizationLatestItemProjectionTests
{
    private readonly Fixture _fixture;
    private readonly EventEnricher _eventEnricher;

    public OrganizationLatestItemProjectionTests()
    {
        var instant = NodaConstants.UnixEpoch;
        var clock = new FakeClock(instant);
        _eventEnricher = EnrichEvent.WithTime(clock);

        _fixture = new Fixture();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeOrganizationOvoCode();

        _fixture.Customize<ImportedOrganization>(
            customization =>
                customization.FromFactory(_ =>
                    {
                        var @event = new ImportedOrganization
                        {
                            Code = _fixture.Create<OrganizationId>(),
                            Name = _fixture.Create<OrganizationName>()
                        };
                        _eventEnricher(@event);

                        return @event;
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
        _eventEnricher(createOrganizationAccepted);

        var expected = new OrganizationLatestItem
        {
            Code = createOrganizationAccepted.Code,
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            IsRemoved = false,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When)
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(createOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_recreated()
    {
        _fixture.Freeze<OrganizationId>();

        var importedOrganization = _fixture.Create<ImportedOrganization>();
        var deleteOrganizationAccepted = new DeleteOrganizationAccepted
        {
            Code = importedOrganization.Code
        };
        _eventEnricher(deleteOrganizationAccepted);

        var createOrganizationAccepted = new CreateOrganizationAccepted {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>(),
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };
        _eventEnricher(createOrganizationAccepted);

        var expected = new OrganizationLatestItem
        {
            Code = createOrganizationAccepted.Code,
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            IsRemoved = false,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When)
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(importedOrganization, deleteOrganizationAccepted, createOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_deleted()
    {
        var createOrganizationAccepted = new CreateOrganizationAccepted
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>(),
        };
        _eventEnricher(createOrganizationAccepted);

        var deleteOrganizationAccepted = new DeleteOrganizationAccepted
        {
            Code = createOrganizationAccepted.Code
        };
        _eventEnricher(deleteOrganizationAccepted);

        var expected = new OrganizationLatestItem
        {
            Code = createOrganizationAccepted.Code,
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            IsRemoved = true,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(deleteOrganizationAccepted.When)
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(createOrganizationAccepted, deleteOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_renamed()
    {
        var importedOrganization = _fixture.Create<ImportedOrganization>();
        var renameOrganizationAccepted = new RenameOrganizationAccepted
        {
            Code = importedOrganization.Code,
            Name = _fixture.CreateWhichIsDifferentThan(new OrganizationName(importedOrganization.Name)),
            When = _fixture.Create<DateTime>().ToString(CultureInfo.InvariantCulture)
        };
        _eventEnricher(renameOrganizationAccepted);

        var expected = new OrganizationLatestItem
        {
            Code = importedOrganization.Code,
            Name = renameOrganizationAccepted.Name,
            OvoCode = null,
            IsRemoved = false,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(renameOrganizationAccepted.When)
        };

        return new OrganizationLatestItemProjection()
            .Scenario()
            .Given(importedOrganization, renameOrganizationAccepted)
            .Expect([expected]);
    }

    [Fact]
    public Task When_organization_is_changed()
    {
        var importedOrganization = _fixture.Create<ImportedOrganization>();
        var changeOrganizationAccepted = new ChangeOrganizationAccepted
        {
            Code = importedOrganization.Code,
            Name = importedOrganization.Name,
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };
        _eventEnricher(changeOrganizationAccepted);

        var expected = new OrganizationLatestItem
        {
            Code = importedOrganization.Code,
            Name = changeOrganizationAccepted.Name,
            OvoCode = changeOrganizationAccepted.OvoCode,
            IsRemoved = false,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(changeOrganizationAccepted.When)
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
            .Select((@event, i) =>
            {
                @event.Code = $"{@event.Code}{i}";

                var expected = new OrganizationLatestItem
                {
                    Code = @event.Code,
                    Name = @event.Name,
                    OvoCode = null,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(@event.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(@event.When)
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
