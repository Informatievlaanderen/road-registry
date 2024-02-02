namespace RoadRegistry.BackOffice.Extensions;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public static class RunnerDbContextExtensions
{
    public static async Task WaitForProjectionToBeAtStoreHeadPosition<TDbContext>(this RunnerDbContext<TDbContext> dbContext, IStreamStore store, string projectionStateName, ILogger logger, CancellationToken cancellationToken)
        where TDbContext : DbContext
    {
        var loggedWaitingMessage = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            var projection = await dbContext.ProjectionStates
                .SingleOrDefaultAsync(item => item.Name == projectionStateName, cancellationToken)
                .ConfigureAwait(false);
            var projectionPosition = projection?.Position;
            var headPosition = await store.ReadHeadPosition(cancellationToken);

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
                loggedWaitingMessage = true;
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
