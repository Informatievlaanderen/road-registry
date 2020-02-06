namespace RoadRegistry.BackOffice.Core
{
    using System;
    using Framework;
    using Messages;
    using NodaTime;
    using SqlStreamStore;

    public class RoadNetworkCommandModule : CommandHandlerModule
    {
        public RoadNetworkCommandModule(IStreamStore store, IRoadNetworkSnapshotReader snapshotReader, IClock clock)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (snapshotReader == null) throw new ArgumentNullException(nameof(snapshotReader));
            if (clock == null) throw new ArgumentNullException(nameof(clock));

            For<ChangeRoadNetworkBasedOnArchive>()
                .UseValidator(new ChangeRoadNetworkBasedOnArchiveValidator())
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var archive = new ArchiveId(message.Body.ArchiveId);
                    var @operator = new OperatorName(message.Body.Operator);
                    var reason = new Reason(message.Body.Reason);
                    var organizationId = new OrganizationId(message.Body.OrganizationId);
                    var organization = await context.Organizations.TryGet(organizationId, ct);
                    var translation = organization == null ? Organization.PredefinedTranslations.Unknown : organization.Translation;

                    var network = await context.RoadNetworks.Get(ct);
                    var translator = new RequestedChangeTranslator(
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
                    var requestedChanges = translator.Translate(message.Body.Changes);
                    network.ChangeBasedOnArchive(archive, reason, @operator, translation, requestedChanges);
                });
        }
    }
}
