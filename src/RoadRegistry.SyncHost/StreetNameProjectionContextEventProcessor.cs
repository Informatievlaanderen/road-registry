namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using Sync.StreetNameRegistry;

public class StreetNameProjectionContextEventProcessor : DbContextEventProcessor<StreetNameProjectionContext>
{
    private const string ProjectionStateName = "roadregistry-sync-streetnameprojection";

    public StreetNameProjectionContextEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<StreetNameProjectionContextEventProcessor, StreetNameProjectionContext> projections,
        EnvelopeFactory envelopeFactory,
        IDbContextFactory<StreetNameProjectionContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<StreetNameProjectionContextEventProcessor> logger)
        : base(ProjectionStateName, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, logger)
    {
    }
}
