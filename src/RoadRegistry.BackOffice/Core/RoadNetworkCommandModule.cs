namespace RoadRegistry.BackOffice.Core;

using System;
using System.Threading.Tasks;
using Autofac;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public class RoadNetworkCommandModule : CommandHandlerModule
{
    public RoadNetworkCommandModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkCommandModule>();
        var enricher = EnrichEvent.WithTime(clock);
        
        For<ChangeRoadNetwork>()
            .UseValidator(new ChangeRoadNetworkValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(ChangeRoadNetwork));

                var request = ChangeRequestId.FromString(message.Body.RequestId);
                var @operator = new OperatorName(message.Body.Operator);
                var reason = new Reason(message.Body.Reason);
                var organizationId = new OrganizationId(message.Body.OrganizationId);
                var organization = await context.Organizations.FindAsync(organizationId, ct);
                var translation = organization == null ? Organization.PredefinedTranslations.Unknown : organization.Translation;

                var network = await context.RoadNetworks.Get(ct);
                var translator = new RequestedChangeTranslator(
                    network.ProvidesNextTransactionId(),
                    network.ProvidesNextRoadNodeId(),
                    network.ProvidesNextRoadNodeVersion(),
                    network.ProvidesNextRoadSegmentId(),
                    network.ProvidesNextGradeSeparatedJunctionId(),
                    network.ProvidesNextEuropeanRoadAttributeId(),
                    network.ProvidesNextNationalRoadAttributeId(),
                    network.ProvidesNextNumberedRoadAttributeId(),
                    network.ProvidesNextRoadSegmentVersion(),
                    network.ProvidesNextRoadSegmentGeometryVersion(),
                    network.ProvidesNextRoadSegmentLaneAttributeId(),
                    network.ProvidesNextRoadSegmentWidthAttributeId(),
                    network.ProvidesNextRoadSegmentSurfaceAttributeId()
                );
                var requestedChanges = await translator.Translate(message.Body.Changes, context.Organizations, ct);

                network.Change(request, reason, @operator, translation, requestedChanges);

                logger.LogInformation("Command handler finished for {Command}", nameof(ChangeRoadNetwork));
            });

        For<CreateOrganization>()
            .UseValidator(new CreateOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(CreateOrganization));

                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.FindAsync(organizationId, ct);

                if (organization != null)
                {
                    var rejectedCommand = new CreateOrganizationRejected
                    {
                        Code = command.Body.Code,
                        Name = command.Body.Name
                    };
                    enricher(rejectedCommand);

                    await new RoadNetworkCommandQueue(store, commandMetadata)
                        .Write(new Command(rejectedCommand), ct);
                }
                else
                {
                    var acceptedCommand = new CreateOrganizationAccepted
                    {
                        Code = command.Body.Code,
                        Name = command.Body.Name
                    };
                    enricher(acceptedCommand);

                    await new OrganizationCommandQueue(store)
                        .Write(organizationId, new Command(acceptedCommand), ct);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(CreateOrganization));
            });

        For<CreateOrganizationRejected>()
            .Handle((_, _, _) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(CreateOrganizationRejected));
                logger.LogInformation("Command handler finished for {Command}", nameof(CreateOrganizationRejected));
                return Task.CompletedTask;
            });

        For<DeleteOrganization>()
            .UseValidator(new DeleteOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(DeleteOrganization));

                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.FindAsync(organizationId, ct);

                if (organization != null)
                {
                    organization.Delete();
                }
                else
                {
                    var rejectedCommand = new DeleteOrganizationRejected
                    {
                        Code = command.Body.Code
                    };
                    enricher(rejectedCommand);

                    await new RoadNetworkCommandQueue(store, commandMetadata)
                        .Write(new Command(rejectedCommand), ct);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(DeleteOrganization));
            });

        For<DeleteOrganizationRejected>()
            .Handle((_, _, _) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(DeleteOrganizationRejected));
                logger.LogInformation("Command handler finished for {Command}", nameof(DeleteOrganizationRejected));
                return Task.CompletedTask;
            });

        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RenameOrganization));

                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.FindAsync(organizationId, ct);

                if (organization != null)
                {
                    organization.Rename(command.Body.Name);
                }
                else
                {
                    var rejectedCommand = new RenameOrganizationRejected
                    {
                        Code = command.Body.Code,
                        Name = command.Body.Name
                    };
                    enricher(rejectedCommand);

                    await new RoadNetworkCommandQueue(store, commandMetadata)
                        .Write(new Command(rejectedCommand), ct);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(RenameOrganization));
            });

        For<RenameOrganizationRejected>()
            .Handle((_, _, _) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", nameof(RenameOrganizationRejected));
                logger.LogInformation("Command handler finished for {Command}", nameof(RenameOrganizationRejected));
                return Task.CompletedTask;
            });
    }
}
