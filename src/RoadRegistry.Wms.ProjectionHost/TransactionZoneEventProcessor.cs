namespace RoadRegistry.Wms.ProjectionHost
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Schema;
    using SqlStreamStore;

    public class TransactionZoneEventProcessor : RunnerDbContextEventProcessor<WmsContext>
    {
        private const string ProjectionStateName = "roadregistry-wms-transactionzone-projectionhost";

        public TransactionZoneEventProcessor(
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
}
