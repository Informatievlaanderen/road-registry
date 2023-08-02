namespace RoadRegistry.Editor.Schema.Extensions;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Metrics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public static class EventProcessorExtensions
{
    public static async Task<EventProcessorMetricsRecord> GetMetricsAsync(this DbSet<EventProcessorMetricsRecord> dbSet, string eventProcessorId, CancellationToken cancellationToken)
    {
        var eventProcessorMetrics = await dbSet
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
                new SqlParameter("@EventProcessorId", SqlDbType.NVarChar) { Value = eventProcessorId },
                new SqlParameter("@DbContextName", SqlDbType.NVarChar) { Value = nameof(EditorContext) }
            ).SingleOrDefaultAsync();
        return eventProcessorMetrics;
    }

    public static async Task<IReadOnlyCollection<EventProcessorMetricsRecord>> GetMetricsAsync(this DbSet<EventProcessorMetricsRecord> dbSet, CancellationToken cancellationToken)
    {
        var eventProcessorMetrics = await dbSet
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
		                AND DbContext = @DbContextName
                GROUP BY EventProcessorId, DbContext
                ",
                new SqlParameter("@DummyId", SqlDbType.UniqueIdentifier) { Value = Guid.Empty },
                new SqlParameter("@DbContextName", SqlDbType.NVarChar) { Value = nameof(EditorContext) }
            ).ToListAsync();
        return eventProcessorMetrics;
    }
}
