namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadNodeEventProcessor : RunnerDbContextEventProcessor<RoadNodeProducerSnapshotContext>
{
    private const string ProjectionStateName = "roadregistry-producer-roadnode-snapshot-projectionhost";

    public RoadNodeEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<RoadNodeProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<RoadNodeProducerSnapshotContext> resolver,
        IDbContextFactory<RoadNodeProducerSnapshotContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base(
            ProjectionStateName,
            streamStore,
            acceptStreamMessage,
            envelopeFactory,
            resolver,
            dbContextFactory,
            scheduler,
            loggerFactory,
            configuration,
            1,
            1)
    {
    }
}
