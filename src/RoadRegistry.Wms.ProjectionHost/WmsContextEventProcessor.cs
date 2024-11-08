namespace RoadRegistry.Wms.ProjectionHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class WmsContextEventProcessor : RunnerDbContextEventProcessor<WmsContext>
{
    private const string ProjectionStateName = "roadregistry-wms-projectionhost";

    public WmsContextEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<WmsContext> filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<WmsContext> resolver,
        IDbContextFactory<WmsContext> dbContextFactory,
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
