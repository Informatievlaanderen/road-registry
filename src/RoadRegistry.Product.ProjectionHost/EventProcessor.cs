namespace RoadRegistry.Product.ProjectionHost
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Microsoft.Extensions.Logging;
    using Schema;
    using SqlStreamStore;

    public class EventProcessor : DbContextEventProcessor<ProductContext>
    {
        private const string QueueName = "roadregistry-product-projectionhost";

        public EventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<ProductContext> resolver,
            Func<ProductContext> dbContextFactory,
            Scheduler scheduler,
            ILogger<EventProcessor> logger)
            : base(QueueName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, logger)
        { }
    }
}
