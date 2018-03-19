namespace RoadRegistry.Api.Oslo.Infrastructure.ETag
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Projections.Oslo;

    public static class GetProjectionPositionExtension
    {
        public static Task<long> GetProjectionPositionAsync(this OsloContext context, CancellationToken cancellationToken)
            => context.GetProjectionPositionAsync(RoadOsloRunner.RunnerName, cancellationToken);

        public static async Task<long> GetProjectionPositionAsync(this OsloContext context, string projectionName, CancellationToken cancellationToken)
        {
            var projectionState =
                await context
                    .ProjectionStates
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.Name == projectionName, cancellationToken);

            return projectionState?.Position ?? -1L;
        }
    }
}
