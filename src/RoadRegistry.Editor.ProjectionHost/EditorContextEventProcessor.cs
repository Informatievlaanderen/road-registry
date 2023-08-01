namespace RoadRegistry.Editor.ProjectionHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using RoadRegistry.BackOffice.Metrics;
using Schema;
using SqlStreamStore;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

public class EditorContextEventProcessor : DbContextEventProcessor<EditorContext>
{
    public EditorContextEventProcessor(string queueName, IStreamStore streamStore, AcceptStreamMessage<EditorContext> acceptStreamMessage, EnvelopeFactory envelopeFactory, ConnectedProjectionHandlerResolver<EditorContext> resolver, IDbContextFactory<EditorContext> dbContextFactory, Scheduler scheduler, ILogger<DbContextEventProcessor<EditorContext>> logger, int catchUpBatchSize = 500, int catchUpThreshold = 1000) : base(queueName, streamStore, acceptStreamMessage, envelopeFactory, resolver, dbContextFactory, scheduler, logger, catchUpBatchSize, catchUpThreshold)
    {
    }

    public EditorContextEventProcessor(string queueName, IStreamStore streamStore, AcceptStreamMessageFilter filter, EnvelopeFactory envelopeFactory, ConnectedProjectionHandlerResolver<EditorContext> resolver, Func<EditorContext> dbContextFactory, Scheduler scheduler, ILogger<DbContextEventProcessor<EditorContext>> logger, int catchUpBatchSize = 500, int catchUpThreshold = 1000) : base(queueName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, logger, catchUpBatchSize, catchUpThreshold)
    {
    }

    protected override async Task OutputEstimatedTimeRemainingAsync(EditorContext context, ILogger logger, long currentPosition, long lastPosition)
    {
        var eventProcessorMetrics = await context.EventProcessorMetrics
            .FromSqlRaw(@"
                SELECT 
		                @DummyId AS Id, 
		                EventProcessorId, 
		                DbContext, 
		                MIN(FromPosition) AS FromPosition, 
		                MAX(ToPosition) AS ToPosition, 
		                SUM(ElapsedMilliseconds) AS ElapsedMilliseconds
                FROM	RoadRegistryEditorMetrics.EventProcessors
                WHERE	1=1
		                AND EventProcessorId = @EventProcessorId
		                AND DbContext = @DbContextName
                GROUP BY EventProcessorId, DbContext
                ",
                new SqlParameter("@DummyId", SqlDbType.UniqueIdentifier) { Value = Guid.Empty },
                new SqlParameter("@EventProcessorId", SqlDbType.NVarChar) { Value = GetType().Name },
                new SqlParameter("@DbContextName", SqlDbType.NVarChar) { Value = nameof(EditorContext) }
            ).SingleOrDefaultAsync();

        if (eventProcessorMetrics is not null)
        {
            var averageTimePerEvent = eventProcessorMetrics.ElapsedMilliseconds / eventProcessorMetrics.ToPosition;
            var estimatedTimeRemaining = averageTimePerEvent * (lastPosition  - currentPosition);

            logger.LogInformation("{EventProcessor} Estimated time remaining between {CurrentPosition} and {LastPosition} is about {EstimatedTimeRemaining} milliseconds.", GetType().Name, currentPosition, lastPosition, estimatedTimeRemaining);
        }
    }

    protected override async Task UpdateEventProcessorMetricsAsync(EditorContext context, long fromPosition, long toPosition, long elapsedMilliseconds, CancellationToken cancellationToken)
    {
        var latestEventProcessorMetrics = await context.EventProcessorMetrics
            .Where(m => m.EventProcessorId == GetType().Name && m.DbContext == nameof(EditorContext))
            .OrderBy(m => m.ToPosition)
            .LastOrDefaultAsync(cancellationToken);

        if (latestEventProcessorMetrics is null)
        {
            await AddEventProcessorMetricsAsync(cancellationToken);
        }
        else if (latestEventProcessorMetrics.ToPosition < toPosition)
        {
            await AddEventProcessorMetricsAsync(cancellationToken);
        }

        async Task AddEventProcessorMetricsAsync(CancellationToken ct)
        {
            await context.EventProcessorMetrics.AddAsync(new EventProcessorMetricsRecord()
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
