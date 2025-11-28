namespace RoadRegistry.BackOffice.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.RoadNetwork.ValueObjects;
using SqlStreamStore;

public class OrganizationCommandModule : CommandHandlerModule
{
    private readonly IOrganizationEventWriter _organizationEventWriter;
    private readonly ILogger _logger;

    public OrganizationCommandModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _logger = loggerFactory.CreateLogger(GetType());

        var enricher = EnrichEvent.WithTime(clock);
        _organizationEventWriter = new OrganizationEventWriter(store, enricher);

        For<CreateOrganization>()
            .UseValidator(new CreateOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(CreateOrganization);

        For<DeleteOrganization>()
            .UseValidator(new DeleteOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(DeleteOrganization);

        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(RenameOrganization);

        For<ChangeOrganization>()
            .UseValidator(new ChangeOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(ChangeOrganization);
    }

    private async Task CreateOrganization(IRoadRegistryContext context, Command<CreateOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            var rejectedEvent = new CreateOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode,
                KboNumber = command.Body.KboNumber
            };
            await _organizationEventWriter.WriteAsync(organizationId, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }
        else
        {
            var acceptedEvent = new CreateOrganizationAccepted
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode,
                KboNumber = command.Body.KboNumber
            };
            await _organizationEventWriter.WriteAsync(organizationId, new Event(
                acceptedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task DeleteOrganization(IRoadRegistryContext context, Command<DeleteOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            organization.Delete();
        }
        else
        {
            var rejectedEvent = new DeleteOrganizationRejected
            {
                Code = command.Body.Code
            };
            await _organizationEventWriter.WriteAsync(organizationId, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task RenameOrganization(IRoadRegistryContext context, Command<RenameOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            organization.Rename(new OrganizationName(command.Body.Name));
        }
        else
        {
            var rejectedEvent = new RenameOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name
            };
            await _organizationEventWriter.WriteAsync(organizationId, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task ChangeOrganization(IRoadRegistryContext context, Command<ChangeOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization is not null)
        {
            organization.Change(
                command.Body.Name is not null ? OrganizationName.WithoutExcessLength(command.Body.Name) : null,
                OrganizationOvoCode.FromValue(command.Body.OvoCode),
                OrganizationKboNumber.FromValue(command.Body.KboNumber),
                command.Body.IsMaintainer
            );
        }
        else
        {
            var rejectedEvent = new ChangeOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode,
                KboNumber = command.Body.KboNumber,
                IsMaintainer = command.Body.IsMaintainer
            };
            await _organizationEventWriter.WriteAsync(organizationId, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }
}
