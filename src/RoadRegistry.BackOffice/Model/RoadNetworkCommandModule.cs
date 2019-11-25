namespace RoadRegistry.BackOffice.Model
{
    using System;
    using Framework;
    using Messages;
    using NodaTime;
    using SqlStreamStore;
    using Translation;

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
                    network.ChangeBaseOnArchive(archive, requestedChanges);
                });
        }
    }
}
