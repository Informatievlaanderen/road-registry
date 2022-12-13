namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public class GradeSeparatedJunctionEventProcessor : DbContextEventProcessor<GradeSeparatedJunctionProducerSnapshotContext>
    {
        private const string QueueName = "roadregistry-producer-gradeseparatedjunction-snapshot-projectionhost";

        public GradeSeparatedJunctionEventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<GradeSeparatedJunctionProducerSnapshotContext> resolver,
            Func<GradeSeparatedJunctionProducerSnapshotContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<DbContextEventProcessor<GradeSeparatedJunctionProducerSnapshotContext>> logger)
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
