namespace RoadRegistry.Product.ProjectionHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;
using System;
using Microsoft.Extensions.Configuration;

public abstract class ProductContextEventProcessor : RunnerDbContextEventProcessor<ProductContext>
{
    protected ProductContextEventProcessor(
        string projectionStateName,
        IStreamStore streamStore,
        AcceptStreamMessageFilter filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<ProductContext> resolver,
        Func<ProductContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base(projectionStateName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, loggerFactory, configuration)
    {
    }
}
