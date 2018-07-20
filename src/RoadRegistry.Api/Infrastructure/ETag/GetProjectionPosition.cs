namespace RoadRegistry.Api.Infrastructure.ETag
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public static class GetProjectionPositionExtension
    {
        public static Task<long> GetProjectionPositionAsync(this ShapeContext context, CancellationToken cancellationToken)
            => context.GetProjectionPositionAsync(RoadShapeRunner.RunnerName, cancellationToken);

        public static async Task<long> GetProjectionPositionAsync(this ShapeContext context, string projectionName, CancellationToken cancellationToken)
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
