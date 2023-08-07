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
            .Handle(async (context, command, _, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

                var request = ChangeRequestId.FromString(command.Body.RequestId);
                DownloadId? downloadId = command.Body.DownloadId is not null ? new DownloadId(command.Body.DownloadId.Value) : null;
                var @operator = new OperatorName(command.Body.Operator);
                var reason = new Reason(command.Body.Reason);
                var organizationId = new OrganizationId(command.Body.OrganizationId);
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
                var requestedChanges = await translator.Translate(command.Body.Changes, context.Organizations, ct);

                network.Change(request, downloadId, reason, @operator, translation, requestedChanges);

                logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
            });

        For<CreateOrganization>()
            .UseValidator(new CreateOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.FindAsync(organizationId, ct);

                if (organization != null)
                {
                    var rejectedCommand = new CreateOrganizationRejected
                    {
                        Code = command.Body.Code,
                        Name = command.Body.Name,
                        OvoCode = command.Body.OvoCode
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
                        Name = command.Body.Name,
                        OvoCode = command.Body.OvoCode
                    };
                    enricher(acceptedCommand);

                    await new OrganizationCommandQueue(store)
                        .Write(organizationId, new Command(acceptedCommand), ct);
                }

                logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
            });

        For<CreateOrganizationRejected>()
            .Handle((_, _, _) => Task.CompletedTask);

        For<DeleteOrganization>()
            .UseValidator(new DeleteOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

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

                logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
            });

        For<DeleteOrganizationRejected>()
            .Handle((_, _, _) => Task.CompletedTask);

        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.FindAsync(organizationId, ct);

                if (organization != null)
                {
                    organization.Rename(new OrganizationName(command.Body.Name));
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

                logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
            });

        For<RenameOrganizationRejected>()
            .Handle((_, _, _) => Task.CompletedTask);

        For<ChangeOrganization>()
            .UseValidator(new ChangeOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(async (context, command, commandMetadata, ct) =>
            {
                logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.FindAsync(organizationId, ct);

                if (organization != null)
                {
                    organization.Change(
                        command.Body.Name is not null ? new OrganizationName(command.Body.Name) : null,
                        OrganizationOvoCode.FromValue(command.Body.OvoCode)
                    );
                }
                else
                {
                    var rejectedCommand = new ChangeOrganizationRejected
                    {
                        Code = command.Body.Code,
                        Name = command.Body.Name,
                        OvoCode = command.Body.OvoCode
                    };
                    enricher(rejectedCommand);

                    await new RoadNetworkCommandQueue(store, commandMetadata)
                        .Write(new Command(rejectedCommand), ct);
                }

                logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
            });

        For<ChangeOrganizationRejected>()
            .Handle((_, _, _) => Task.CompletedTask);
    }
}
