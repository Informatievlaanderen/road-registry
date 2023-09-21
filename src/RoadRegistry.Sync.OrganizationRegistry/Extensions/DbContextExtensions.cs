namespace RoadRegistry.Sync.OrganizationRegistry.Extensions;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Microsoft.EntityFrameworkCore;

public static class DbContextExtensions
{
    public static async Task<ProjectionStateItem> InitializeProjectionState<TDbContext>(this RunnerDbContext<TDbContext> context, string name, CancellationToken cancellationToken)
        where TDbContext : DbContext
    {
        var projectionState = await context.ProjectionStates.SingleOrDefaultAsync(cancellationToken)
                              ?? context.ProjectionStates.Local.SingleOrDefault();
        if (projectionState is null)
        {
            projectionState = new ProjectionStateItem
            {
                Name = name
            };
            await context.ProjectionStates.AddAsync(projectionState, cancellationToken);
        }

        return projectionState;
    }
}
