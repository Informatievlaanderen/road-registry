namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public static class RunnerDbContextExtensions
{
    public static bool IsNullOrDeleted<TDbContext>(this RunnerDbContext<TDbContext> dbContext, object item)
        where TDbContext : DbContext
    {
        return item is null || dbContext.Entry(item).State == EntityState.Deleted;
    }
    
    public static async Task WaitForProjectionToBeAtStoreHeadPosition<TDbContext>(this RunnerDbContext<TDbContext> dbContext,
        IStreamStore store,
        string projectionStateName,
        ILogger logger,
        CancellationToken cancellationToken,
        int waitDelayMilliseconds = 500)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(projectionStateName);

        await WaitForProjectionsToBeAtStoreHeadPosition(dbContext, store, [projectionStateName], logger, cancellationToken, waitDelayMilliseconds);
    }

    public static async Task WaitForProjectionsToBeAtStoreHeadPosition<TDbContext>(this RunnerDbContext<TDbContext> dbContext,
        IStreamStore store,
        ICollection<string> projectionStateNames,
        ILogger logger,
        CancellationToken cancellationToken,
        int waitDelayMilliseconds = 500)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(projectionStateNames);

        var loggedWaitingMessages = new Dictionary<string, bool>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var headPosition = await store.ReadHeadPosition(cancellationToken);

            var projections = await dbContext.ProjectionStates
                .IncludeLocalToListAsync(q => q.Where(item => projectionStateNames.Contains(item.Name)), cancellationToken)
                .ConfigureAwait(false);

            foreach (var projectionStateName in projectionStateNames.Distinct())
            {
                var projection = projections.SingleOrDefault(x => x.Name == projectionStateName);
                var projectionPosition = projection?.Position;

                loggedWaitingMessages.TryGetValue(projectionStateName, out var loggedWaitingMessage);

                var editorProjectionIsUpToDate = projectionPosition == headPosition;
                if (editorProjectionIsUpToDate)
                {
                    if (loggedWaitingMessage)
                    {
                        logger.LogInformation("{DbContext} projection queue {ProjectionStateName}: is up-to-date", dbContext.GetType().Name, projectionStateName);
                    }
                    return;
                }

                if (!loggedWaitingMessage)
                {
                    logger.LogInformation("{DbContext} projection queue {ProjectionStateName}: waiting to be up-to-date ...", dbContext.GetType().Name, projectionStateName);
                    loggedWaitingMessages[projectionStateName] = true;
                }
            }

            await Task.Delay(waitDelayMilliseconds, cancellationToken);
        }
    }
}
