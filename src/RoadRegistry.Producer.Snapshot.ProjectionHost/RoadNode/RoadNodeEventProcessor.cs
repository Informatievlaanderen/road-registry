namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadNodeEventProcessor : DbContextEventProcessor<RoadNodeProducerSnapshotContext>
{
    private const string QueueName = "roadregistry-producer-roadnode-snapshot-projectionhost";

    public RoadNodeEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<RoadNodeProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<RoadNodeProducerSnapshotContext> resolver,
        IDbContextFactory<RoadNodeProducerSnapshotContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<DbContextEventProcessor<RoadNodeProducerSnapshotContext>> logger)
        : base(
            QueueName,
            streamStore,
            acceptStreamMessage,
            envelopeFactory,
            resolver,
            dbContextFactory,
            scheduler,
            logger,
            1,
            1)
    {
    }

    protected override Task UpdateEventProcessorMetricsAsync(RoadNodeProducerSnapshotContext context, long fromPosition, long toPosition, long elapsedMilliseconds, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }
}
