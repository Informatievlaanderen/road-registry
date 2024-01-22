namespace RoadRegistry.Sync.OrganizationRegistry.Extensions;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Microsoft.EntityFrameworkCore;
using OrganizationRegistry;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class DbContextExtensions
{
    public static async Task<ProjectionStateItem> InitializeProjectionState(this IProjectionStatesDbContext context, string name, CancellationToken cancellationToken)
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
