namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class GradeSeparatedJunctionEventProcessor : RunnerDbContextEventProcessor<GradeSeparatedJunctionProducerSnapshotContext>
{
    private const string ProjectionStateName = "roadregistry-producer-gradeseparatedjunction-snapshot-projectionhost";

    public GradeSeparatedJunctionEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<GradeSeparatedJunctionProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<GradeSeparatedJunctionProducerSnapshotContext> resolver,
        IDbContextFactory<GradeSeparatedJunctionProducerSnapshotContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory)
        : base(
            ProjectionStateName,
            streamStore,
            acceptStreamMessage,
            envelopeFactory,
            resolver,
            dbContextFactory,
            scheduler,
            loggerFactory,
            1,
            1)
    {
    }
}
