namespace RoadRegistry.Integration.ProjectionHost;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public abstract class IntegrationContextEventProcessor : DbContextEventProcessor<IntegrationContext>
{
    protected IntegrationContextEventProcessor(
        string projectionStateName,
        IStreamStore streamStore,
        AcceptStreamMessage<IntegrationContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<IntegrationContext> resolver,
        IDbContextFactory<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<DbContextEventProcessor<IntegrationContext>> logger,
        int catchUpBatchSize = 500,
        int catchUpThreshold = 1000)
        : base(projectionStateName, streamStore, acceptStreamMessage, envelopeFactory, resolver, dbContextFactory, scheduler, logger, catchUpBatchSize, catchUpThreshold)
    {
    }

    protected IntegrationContextEventProcessor(
        string projectionStateName,
        IStreamStore streamStore,
        AcceptStreamMessageFilter filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<IntegrationContext> resolver,
        Func<IntegrationContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<DbContextEventProcessor<IntegrationContext>> logger,
        int catchUpBatchSize = 500,
        int catchUpThreshold = 1000)
        : base(projectionStateName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, logger, catchUpBatchSize, catchUpThreshold)
    {
    }
}
