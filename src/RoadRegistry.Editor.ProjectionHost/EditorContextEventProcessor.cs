namespace RoadRegistry.Editor.ProjectionHost;

using BackOffice.Metrics;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema;
using Schema.Extensions;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public abstract class EditorContextEventProcessor : RunnerDbContextEventProcessor<EditorContext>
{
    protected EditorContextEventProcessor(string projectionStateName, IStreamStore streamStore, AcceptStreamMessage<EditorContext> acceptStreamMessage, EnvelopeFactory envelopeFactory, ConnectedProjectionHandlerResolver<EditorContext> resolver, IDbContextFactory<EditorContext> dbContextFactory, Scheduler scheduler, ILoggerFactory loggerFactory, IConfiguration configuration, int catchUpBatchSize = 500, int catchUpThreshold = 1000)
        : base(projectionStateName, streamStore, acceptStreamMessage, envelopeFactory, resolver, dbContextFactory, scheduler, loggerFactory, configuration, catchUpBatchSize, catchUpThreshold)
    {
    }

    protected EditorContextEventProcessor(string projectionStateName, IStreamStore streamStore, AcceptStreamMessageFilter filter, EnvelopeFactory envelopeFactory, ConnectedProjectionHandlerResolver<EditorContext> resolver, Func<EditorContext> dbContextFactory, Scheduler scheduler, ILoggerFactory loggerFactory, IConfiguration configuration, int catchUpBatchSize = 500, int catchUpThreshold = 1000)
        : base(projectionStateName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, loggerFactory, configuration, catchUpBatchSize, catchUpThreshold)
    {
    }

    protected override async Task OutputEstimatedTimeRemainingAsync(EditorContext context, long currentPosition, long lastPosition, CancellationToken cancellationToken)
    {
        var eventProcessorMetrics = await context.EventProcessorMetrics.GetMetricsAsync(GetType().Name, currentPosition, cancellationToken);

        if (eventProcessorMetrics is not null)
        {
            var estimatedTimeRemaining = eventProcessorMetrics.ElapsedMilliseconds;

            Logger.LogInformation("Estimated time remaining between {CurrentPosition} and {LastPosition} is about {EstimatedTimeRemaining} milliseconds.", currentPosition, lastPosition, estimatedTimeRemaining);
        }
    }

    protected override async Task UpdateEventProcessorMetricsAsync(EditorContext context, long fromPosition, long toPosition, long elapsedMilliseconds, CancellationToken cancellationToken)
    {
        var eventProcessorMetrics = await context.EventProcessorMetrics.GetMetricsAsync(GetType().Name, toPosition, cancellationToken);

        if (eventProcessorMetrics is null)
        {
            await AddEventProcessorMetricsAsync(cancellationToken);
        }
        else if (eventProcessorMetrics.ToPosition < toPosition)
        {
            await AddEventProcessorMetricsAsync(cancellationToken);
        }

        async Task AddEventProcessorMetricsAsync(CancellationToken ct)
        {
            await context.EventProcessorMetrics.AddAsync(new EventProcessorMetricsRecord
            {
                Id = Guid.NewGuid(),
                EventProcessorId = GetType().Name,
                DbContext = nameof(EditorContext),
                FromPosition = fromPosition,
                ToPosition = toPosition,
                ElapsedMilliseconds = elapsedMilliseconds
            }, ct);
        }
    }
}
