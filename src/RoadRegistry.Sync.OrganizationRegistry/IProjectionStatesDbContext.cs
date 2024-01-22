namespace RoadRegistry.Sync.OrganizationRegistry;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Microsoft.EntityFrameworkCore;

public interface IProjectionStatesDbContext
{
    DbSet<ProjectionStateItem> ProjectionStates { get; }
}
