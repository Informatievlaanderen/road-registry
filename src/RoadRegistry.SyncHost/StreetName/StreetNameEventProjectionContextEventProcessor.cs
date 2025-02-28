namespace RoadRegistry.SyncHost.StreetName;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoadRegistry.Hosts;
using RoadRegistry.Sync.StreetNameRegistry;
using SqlStreamStore;

public class StreetNameEventProjectionContextEventProcessor : RunnerDbContextEventProcessor<StreetNameEventProjectionContext>
{
    private const string ProjectionStateName = "roadregistry-sync-streetnameeventprojection";

    public StreetNameEventProjectionContextEventProcessor(
        IStreamStore streamStore,
        DbContextEventProcessorProjections<StreetNameEventProjectionContextEventProcessor, StreetNameEventProjectionContext> projections,
        EnvelopeFactory envelopeFactory,
        IDbContextFactory<StreetNameEventProjectionContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base(ProjectionStateName, streamStore, projections.Filter, envelopeFactory, projections.Resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
