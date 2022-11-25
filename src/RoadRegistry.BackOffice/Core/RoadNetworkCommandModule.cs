namespace RoadRegistry.BackOffice.Core;

using System;
using System.Threading.Tasks;
using Framework;
using Messages;
using NodaTime;
using SqlStreamStore;
using SqlStreamStore.Streams;

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
            });

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

        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, snapshotReader, enricher)
            .Handle(async (context, command, ct) =>
            {
                var organizationId = new OrganizationId(command.Body.Code);
                var organization = await context.Organizations.TryGet(organizationId, ct);

                if (organization != null)
                {
                    organization.Rename(command.Body.Name);
                }
                else
                {
                    var rejectCommand = new RenameOrganizationRejected
                    {
                        Code = command.Body.Code,
                        Name = command.Body.Name
                    };
                    enricher(rejectCommand);

                    await new RoadNetworkCommandQueue(store)
                        .Write(new Command(rejectCommand), ct);
                }
            });

        For<RenameOrganizationRejected>()
            .Handle((_, _) => Task.CompletedTask);
    }
}
