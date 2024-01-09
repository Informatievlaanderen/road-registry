namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions
{
    using Core;
    using FeatureToggles;

    internal static class RoadNetworkExtensions
    {
        public static async Task<RoadSegment?> FindRoadSegment(this IRoadNetworks roadNetworks,
            RoadSegmentId id,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            UseDefaultRoadNetworkFallbackForOutlinedRoadSegmentsFeatureToggle useDefaultRoadNetworkFallbackForOutlinedRoadSegmentsFeatureToggle,
            CancellationToken cancellationToken)
        {
            if (geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
            {
                var roadNetwork = await roadNetworks.Get(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(id), cancellationToken);

                var roadSegment = roadNetwork.FindRoadSegment(id);
                if (roadSegment is not null)
                {
                    return roadSegment;
                }

                if (!useDefaultRoadNetworkFallbackForOutlinedRoadSegmentsFeatureToggle.FeatureEnabled)
                {
                    return null;
                }
            }

            {
                var roadNetwork = await roadNetworks.Get(RoadNetworkStreamNameProvider.Default, cancellationToken);

                return roadNetwork.FindRoadSegment(id);
            }
        }
    }
}
