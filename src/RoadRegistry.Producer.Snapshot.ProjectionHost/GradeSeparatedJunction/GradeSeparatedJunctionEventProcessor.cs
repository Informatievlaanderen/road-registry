namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class GradeSeparatedJunctionEventProcessor : DbContextEventProcessor<GradeSeparatedJunctionProducerSnapshotContext>
{
    private const string QueueName = "roadregistry-producer-gradeseparatedjunction-snapshot-projectionhost";

    public GradeSeparatedJunctionEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<GradeSeparatedJunctionProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<GradeSeparatedJunctionProducerSnapshotContext> resolver,
        IDbContextFactory<GradeSeparatedJunctionProducerSnapshotContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<DbContextEventProcessor<GradeSeparatedJunctionProducerSnapshotContext>> logger)
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

    protected override Task UpdateEventProcessorMetricsAsync(GradeSeparatedJunctionProducerSnapshotContext context, long fromPosition, long toPosition, long elapsedMilliseconds, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }
}
