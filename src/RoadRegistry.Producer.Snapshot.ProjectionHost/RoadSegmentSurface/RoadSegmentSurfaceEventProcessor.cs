namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class RoadSegmentSurfaceEventProcessor : DbContextEventProcessor<RoadSegmentSurfaceProducerSnapshotContext>
{
    private const string ProjectionStateName = "roadregistry-producer-roadsegmentsurface-snapshot-projectionhost";

    public RoadSegmentSurfaceEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<RoadSegmentSurfaceProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<RoadSegmentSurfaceProducerSnapshotContext> resolver,
        IDbContextFactory<RoadSegmentSurfaceProducerSnapshotContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<DbContextEventProcessor<RoadSegmentSurfaceProducerSnapshotContext>> logger)
        : base(
            ProjectionStateName,
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
}
