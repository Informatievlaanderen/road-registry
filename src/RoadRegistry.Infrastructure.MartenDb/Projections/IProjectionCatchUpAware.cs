namespace RoadRegistry.Infrastructure.MartenDb.Projections;

// Implemented by a sub-projection that wants to know when its driver starts and finishes catching up - for instance to
// hold a lookup table in memory for the duration of a rebuild instead of querying it once per event, and to let go of
// it again afterwards. Purely optional: a sub-projection that does not implement it is never called.
public interface IProjectionCatchUpAware
{
    Task OnCatchUpStartedAsync(CancellationToken cancellationToken);

    Task OnCatchUpFinishedAsync(CancellationToken cancellationToken);
}
