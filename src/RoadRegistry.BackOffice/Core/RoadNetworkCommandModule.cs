namespace RoadRegistry.BackOffice.Core;

using System;
using System.Threading.Tasks;
using Framework;
using Messages;
using NodaTime;
using SqlStreamStore;

public class RoadNetworkCommandModule : CommandHandlerModule
{
    public RoadNetworkCommandModule(
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);

        var enricher = EnrichEvent.WithTime(clock);

        For<RebuildRoadNetworkSnapshot>()
            .UseRoadRegistryContext(store, snapshotReader, enricher)
            .Handle(async (context, command, ct) =>
            {
                await snapshotWriter.SetHeadToVersion(command.Body.StartFromVersion, ct);
                var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                var completedCommand = new RebuildRoadNetworkSnapshotCompleted
                {
                    StartFromVersion = command.Body.StartFromVersion,
                    CurrentVersion = version
                };

                await new RoadNetworkCommandQueue(store)
                    .Write(new Command(completedCommand), ct);
            });

        For<RebuildRoadNetworkSnapshotCompleted>()
            .Handle((_, _) => Task.CompletedTask);

        For<ChangeRoadNetwork>()
            .UseValidator(new ChangeRoadNetworkValidator())
            .UseRoadRegistryContext(store, snapshotReader, enricher)
            .Handle(async (context, message, ct) =>
            {
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
                    network.ProvidesNextRoadSegmentId(),
                    network.ProvidesNextGradeSeparatedJunctionId(),
                    network.ProvidesNextEuropeanRoadAttributeId(),
                    network.ProvidesNextNationalRoadAttributeId(),
                    network.ProvidesNextNumberedRoadAttributeId(),
                    network.ProvidesNextRoadSegmentLaneAttributeId(),
                    network.ProvidesNextRoadSegmentWidthAttributeId(),
                    network.ProvidesNextRoadSegmentSurfaceAttributeId()
                );
                var requestedChanges = await translator.Translate(message.Body.Changes, context.Organizations, ct);
                network.Change(request, reason, @operator, translation, requestedChanges);
            });

        For<CreateOrganization>()
            .UseValidator(new CreateOrganizationValidator())
            .UseRoadRegistryContext(store, snapshotReader, enricher)
            .Handle(async (context, command, ct) =>
            {
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

                    await new RoadNetworkCommandQueue(store)
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
            });

        For<CreateOrganizationRejected>()
            .Handle((_, _) => Task.CompletedTask);

        For<DeleteOrganization>()
            .UseValidator(new DeleteOrganizationValidator())
            .UseRoadRegistryContext(store, snapshotReader, enricher)
            .Handle(async (context, command, ct) =>
            {
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

                    await new RoadNetworkCommandQueue(store)
                        .Write(new Command(rejectedCommand), ct);
                }
            });

        For<DeleteOrganizationRejected>()
            .Handle((_, _) => Task.CompletedTask);

        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, snapshotReader, enricher)
            .Handle(async (context, command, ct) =>
            {
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

                    await new RoadNetworkCommandQueue(store)
                        .Write(new Command(rejectedCommand), ct);
                }
            });

        For<RenameOrganizationRejected>()
            .Handle((_, _) => Task.CompletedTask);
    }
}
