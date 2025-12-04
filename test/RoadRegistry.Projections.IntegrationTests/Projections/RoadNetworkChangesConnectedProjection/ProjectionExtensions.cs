namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using RoadRegistry.Projections.IntegrationTests.Infrastructure;
using RoadRegistry.RoadSegment.ValueObjects;

public static class ProjectionExtensions
{
    public static Task Expect(this MartenProjectionIntegrationTestRunner runner, params RoadSegmentProjectionItem[] expectedDocuments)
    {
        return runner.Expect(expectedDocuments.Select(x => (x.Id, (RoadSegmentProjectionItem?)x)).ToArray());
    }

    public static Task ExpectNone(this MartenProjectionIntegrationTestRunner runner, params RoadSegmentId[] roadSegmentIds)
    {
        return runner.Expect(roadSegmentIds.Select(x => (x.ToInt32(), (RoadSegmentProjectionItem?)null)).ToArray());
    }
}
