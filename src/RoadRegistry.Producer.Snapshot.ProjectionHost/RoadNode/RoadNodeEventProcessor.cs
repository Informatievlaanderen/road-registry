namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public class RoadNodeEventProcessor : DbContextEventProcessor<RoadNodeProducerSnapshotContext>
    {
        private const string QueueName = "roadregistry-producer-roadnode-snapshot-projectionhost";

        public RoadNodeEventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<RoadNodeProducerSnapshotContext> resolver,
            Func<RoadNodeProducerSnapshotContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<DbContextEventProcessor<RoadNodeProducerSnapshotContext>> logger)
            : base(
                QueueName,
                streamStore,
                filter,
                envelopeFactory,
                resolver,
                dbContextFactory,
                scheduler,
                logger,
                catchUpBatchSize: 1,
                catchUpThreshold: 1)
        { }
    }
}
