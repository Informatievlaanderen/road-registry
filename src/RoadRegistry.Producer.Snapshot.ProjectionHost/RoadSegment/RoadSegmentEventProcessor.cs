namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public class RoadSegmentEventProcessor : DbContextEventProcessor<RoadSegmentProducerSnapshotContext>
    {
        private const string QueueName = "roadregistry-producer-roadsegment-snapshot-projectionhost";

        public RoadSegmentEventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessage<RoadSegmentProducerSnapshotContext> acceptStreamMessage,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<RoadSegmentProducerSnapshotContext> resolver,
            IDbContextFactory<RoadSegmentProducerSnapshotContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<DbContextEventProcessor<RoadSegmentProducerSnapshotContext>> logger)
            : base(
                QueueName,
                streamStore,
                acceptStreamMessage,
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
