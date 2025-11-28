namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions
{
    using Core;
    using RoadRegistry.RoadNetwork.ValueObjects;

    internal static class RoadNetworkExtensions
    {
        public static async Task<RoadSegment?> FindRoadSegment(this IRoadNetworks roadNetworks,
            RoadSegmentId id,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            CancellationToken cancellationToken)
        {
            var roadNetwork = await roadNetworks.Get(RoadNetworkStreamNameProvider.Get(id, geometryDrawMethod), cancellationToken);

            var roadSegment = roadNetwork.FindRoadSegment(id);
            return roadSegment;
        }
    }
}
