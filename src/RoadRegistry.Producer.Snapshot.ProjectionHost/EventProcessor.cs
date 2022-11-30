namespace RoadRegistry.Producer.Snapshot.ProjectionHost
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.Extensions.Logging;
    using Schema;
    using SqlStreamStore;

    public class EventProcessor : DbContextEventProcessor<ProducerSnapshotContext>
    {
        private const string QueueName = "roadregistry-producer-snapshot-projectionhost";

        public EventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<ProducerSnapshotContext> resolver,
            Func<ProducerSnapshotContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<DbContextEventProcessor<ProducerSnapshotContext>> logger)
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
