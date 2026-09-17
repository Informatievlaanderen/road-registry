namespace RoadRegistry.Infrastructure.MartenDb.Projections;

// A sub-projection whose read model needs rows no event brings. The driver initializes it when the projection starts
// from nothing - a new read model, or one a rebuild emptied - before the first events are applied.
public interface IInitializableRoadNetworkChangesProjection<in TSession>
{
    Task InitializeAsync(TSession session, CancellationToken cancellationToken);
}
