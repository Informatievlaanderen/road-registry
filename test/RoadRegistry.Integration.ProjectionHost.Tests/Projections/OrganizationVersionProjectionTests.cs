namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using System.Globalization;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using Integration.Projections.Version;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.Tests.BackOffice;
using Schema.Organizations.Version;

public class OrganizationVersionProjectionTests
{
    private const long InitialPosition = IntegrationContextScenarioExtensions.InitialPosition;
    private readonly Fixture _fixture;
    private readonly EventEnricher _eventEnricher;

    public OrganizationVersionProjectionTests()
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
        var createOrganizationAccepted = new CreateOrganizationAccepted
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>(),
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };
        _eventEnricher(createOrganizationAccepted);

        var expected = new OrganizationVersion
        {
            Position = InitialPosition,
            Code = createOrganizationAccepted.Code,
            Name = createOrganizationAccepted.Name,
            OvoCode = createOrganizationAccepted.OvoCode,
            IsRemoved = false,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When)
        };

        return new OrganizationVersionProjection()
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

        var createOrganizationAccepted = new CreateOrganizationAccepted
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.Create<OrganizationName>(),
            OvoCode = _fixture.Create<OrganizationOvoCode>()
        };
        _eventEnricher(createOrganizationAccepted);

        var initialPosition = InitialPosition;

        return new OrganizationVersionProjection()
            .Scenario()
            .Given(importedOrganization, deleteOrganizationAccepted, createOrganizationAccepted)
            .Expect([
                new OrganizationVersion
                {
                    Position = initialPosition,
                    Code = importedOrganization.Code,
                    Name = importedOrganization.Name,
                    OvoCode = null,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When)
                },
                new OrganizationVersion
                {
                    Position = ++initialPosition,
                    Code = importedOrganization.Code,
                    Name = importedOrganization.Name,
                    OvoCode = null,
                    IsRemoved = true,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(deleteOrganizationAccepted.When)
                },
                new OrganizationVersion
                {
                    Position = ++initialPosition,
                    Code = importedOrganization.Code,
                    Name = createOrganizationAccepted.Name,
                    OvoCode = createOrganizationAccepted.OvoCode,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When)
                }
            ]);
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

        var initialPosition = InitialPosition;

        return new OrganizationVersionProjection()
            .Scenario()
            .Given(createOrganizationAccepted, deleteOrganizationAccepted)
            .Expect([new OrganizationVersion
                {
                    Position = initialPosition,
                    Code = createOrganizationAccepted.Code,
                    Name = createOrganizationAccepted.Name,
                    OvoCode = createOrganizationAccepted.OvoCode,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When)
                },new OrganizationVersion
                {
                    Position = ++initialPosition,
                    Code = createOrganizationAccepted.Code,
                    Name = createOrganizationAccepted.Name,
                    OvoCode = createOrganizationAccepted.OvoCode,
                    IsRemoved = true,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(createOrganizationAccepted.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(deleteOrganizationAccepted.When)
                }
            ]);
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

        var initialPosition = InitialPosition;

        return new OrganizationVersionProjection()
            .Scenario()
            .Given(importedOrganization, renameOrganizationAccepted)
            .Expect([
                new OrganizationVersion
                {
                    Position = initialPosition,
                    Code = importedOrganization.Code,
                    Name = importedOrganization.Name,
                    OvoCode = null,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When)
                },
                new OrganizationVersion
                {
                    Position = ++initialPosition,
                    Code = importedOrganization.Code,
                    Name = renameOrganizationAccepted.Name,
                    OvoCode = null,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(renameOrganizationAccepted.When)
                }
            ]);
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

        var initialPosition = InitialPosition;

        return new OrganizationVersionProjection()
            .Scenario()
            .Given(importedOrganization, changeOrganizationAccepted)
            .Expect([
                new OrganizationVersion
                {
                    Position = initialPosition,
                    Code = importedOrganization.Code,
                    Name = importedOrganization.Name,
                    OvoCode = null,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When)
                },
                new OrganizationVersion
                {
                    Position = ++initialPosition,
                    Code = importedOrganization.Code,
                    Name = changeOrganizationAccepted.Name,
                    OvoCode = changeOrganizationAccepted.OvoCode,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(importedOrganization.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(changeOrganizationAccepted.When)
                }]);
    }

    [Fact]
    public Task When_organizations_are_imported()
    {
        var initialPosition = InitialPosition;

        var data = _fixture
            .CreateMany<ImportedOrganization>(new Random().Next(1, 100))
            .Select((@event, i) =>
            {
                @event.Code = $"{@event.Code}{i}";

                var expected = new OrganizationVersion
                {
                    Position = initialPosition,
                    Code = @event.Code,
                    Name = @event.Name,
                    OvoCode = null,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(@event.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(@event.When)
                };

                initialPosition++;

                return new
                {
                    ImportedOrganization = @event,
                    Expected = expected
                };
            }).ToList();

        return new OrganizationVersionProjection()
            .Scenario()
            .Given(data.Select(d => d.ImportedOrganization))
            .Expect(data.Select(d => d.Expected));
    }
}
