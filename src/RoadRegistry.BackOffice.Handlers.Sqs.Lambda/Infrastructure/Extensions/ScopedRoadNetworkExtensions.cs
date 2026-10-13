namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;

using RoadRegistry.ScopedRoadNetwork;

internal static class ScopedRoadNetworkExtensions
{
    // The V2 road segments among the given ones, as the loaded road network knows them. This is what the
    // inwinningsstatus asks about a road segment that takes part in no inwinning: a V2 one is 'compleet' all the same.
    public static Func<IReadOnlyCollection<RoadSegmentId>, CancellationToken, Task<IReadOnlyCollection<RoadSegmentId>>> SelectMigratedRoadSegmentIds(
        this ScopedRoadNetwork roadNetwork)
    {
        return (roadSegmentIds, _) => Task.FromResult<IReadOnlyCollection<RoadSegmentId>>(roadSegmentIds
            .Where(x => roadNetwork.RoadSegments.TryGetValue(x, out var roadSegment) && roadSegment.HasMigrated())
            .ToList());
    }
}
