namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
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
        AcceptStreamMessage<StreetNameProjectionContext> filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<StreetNameProjectionContext> resolver,
        IDbContextFactory<StreetNameProjectionContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<StreetNameProjectionContextEventProcessor> logger)
        : base(ProjectionStateName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, logger)
    {
    }
}
