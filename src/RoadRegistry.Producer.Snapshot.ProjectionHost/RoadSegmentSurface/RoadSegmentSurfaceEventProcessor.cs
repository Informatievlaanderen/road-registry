namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public class RoadSegmentSurfaceEventProcessor : DbContextEventProcessor<RoadSegmentSurfaceProducerSnapshotContext>
    {
        private const string QueueName = "roadregistry-producer-roadsegmentsurface-snapshot-projectionhost";

        public RoadSegmentSurfaceEventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<RoadSegmentSurfaceProducerSnapshotContext> resolver,
            Func<RoadSegmentSurfaceProducerSnapshotContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<DbContextEventProcessor<RoadSegmentSurfaceProducerSnapshotContext>> logger)
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
