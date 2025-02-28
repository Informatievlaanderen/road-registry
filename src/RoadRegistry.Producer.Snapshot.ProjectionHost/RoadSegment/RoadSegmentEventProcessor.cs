namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadSegmentEventProcessor : RunnerDbContextEventProcessor<RoadSegmentProducerSnapshotContext>
{
    private const string ProjectionStateName = "roadregistry-producer-roadsegment-snapshot-projectionhost";

    public RoadSegmentEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<RoadSegmentProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<RoadSegmentProducerSnapshotContext> resolver,
        IDbContextFactory<RoadSegmentProducerSnapshotContext> dbContextFactory,
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
