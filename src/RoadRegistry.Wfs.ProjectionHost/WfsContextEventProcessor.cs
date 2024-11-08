namespace RoadRegistry.Wfs.ProjectionHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class WfsContextEventProcessor : RunnerDbContextEventProcessor<WfsContext>
{
    private const string ProjectionStateName = "roadregistry-wfs-projectionhost";

    public WfsContextEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<WfsContext> filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<WfsContext> resolver,
        IDbContextFactory<WfsContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory)
        : base(
            ProjectionStateName,
            streamStore,
            filter,
            envelopeFactory,
            resolver,
            dbContextFactory,
            scheduler,
            loggerFactory)
    {
    }
}
