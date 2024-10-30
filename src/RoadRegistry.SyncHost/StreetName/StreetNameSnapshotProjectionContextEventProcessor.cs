namespace RoadRegistry.SyncHost.StreetName;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Hosts;
using RoadRegistry.Sync.StreetNameRegistry;
using SqlStreamStore;

public class StreetNameSnapshotProjectionContextEventProcessor : DbContextEventProcessor<StreetNameSnapshotProjectionContext>
{
    private const string ProjectionStateName = "roadregistry-sync-streetnameprojection";

    public StreetNameSnapshotProjectionContextEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<StreetNameSnapshotProjectionContextEventProcessor, StreetNameSnapshotProjectionContext> projections,
        EnvelopeFactory envelopeFactory,
        IDbContextFactory<StreetNameSnapshotProjectionContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<StreetNameSnapshotProjectionContextEventProcessor> logger)
        : base(ProjectionStateName, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
