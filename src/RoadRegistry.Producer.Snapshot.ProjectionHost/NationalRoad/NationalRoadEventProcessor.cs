namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public class NationalRoadEventProcessor : DbContextEventProcessor<NationalRoadProducerSnapshotContext>
    {
        private const string QueueName = "roadregistry-producer-nationalroad-snapshot-projectionhost";

        public NationalRoadEventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<NationalRoadProducerSnapshotContext> resolver,
            Func<NationalRoadProducerSnapshotContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<DbContextEventProcessor<NationalRoadProducerSnapshotContext>> logger)
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
