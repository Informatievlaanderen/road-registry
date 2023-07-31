namespace RoadRegistry.Editor.ProjectionHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;

public class EditorContextEventProcessor : DbContextEventProcessor<EditorContext>
{
    public EditorContextEventProcessor(string queueName, IStreamStore streamStore, AcceptStreamMessage<EditorContext> acceptStreamMessage, EnvelopeFactory envelopeFactory, ConnectedProjectionHandlerResolver<EditorContext> resolver, IDbContextFactory<EditorContext> dbContextFactory, Scheduler scheduler, ILogger<DbContextEventProcessor<EditorContext>> logger, int catchUpBatchSize = 500, int catchUpThreshold = 1000) : base(queueName, streamStore, acceptStreamMessage, envelopeFactory, resolver, dbContextFactory, scheduler, logger, catchUpBatchSize, catchUpThreshold)
    {
    }

    public EditorContextEventProcessor(string queueName, IStreamStore streamStore, AcceptStreamMessageFilter filter, EnvelopeFactory envelopeFactory, ConnectedProjectionHandlerResolver<EditorContext> resolver, Func<EditorContext> dbContextFactory, Scheduler scheduler, ILogger<DbContextEventProcessor<EditorContext>> logger, int catchUpBatchSize = 500, int catchUpThreshold = 1000) : base(queueName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, logger, catchUpBatchSize, catchUpThreshold)
    {
    }

    protected override Task UpdateEventProcessorMetricsAsync(CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }
}
