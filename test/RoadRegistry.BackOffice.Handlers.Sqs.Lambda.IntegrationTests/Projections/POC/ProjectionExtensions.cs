namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections.POC;

using RoadSegment.ValueObjects;

public static class ProjectionExtensions
{
    public static Task Expect(this MartenProjectionTestRunner runner, params RoadNodeProjectionItem[] expectedDocuments)
    {
        return runner.Expect(expectedDocuments.Select(x => (x.Id, (RoadNodeProjectionItem?)x)).ToArray());
    }
    public static Task ExpectNone(this MartenProjectionTestRunner runner, RoadNodeId roadNodeId)
    {
        return runner.Expect([(roadNodeId, (RoadNodeProjectionItem?)null)]);
    }

    public static Task Expect(this MartenProjectionTestRunner runner, params RoadSegmentProjectionItem[] expectedDocuments)
    {
        return runner.Expect(expectedDocuments.Select(x => (x.Id, (RoadSegmentProjectionItem?)x)).ToArray());
    }
    public static Task ExpectNone(this MartenProjectionTestRunner runner, RoadSegmentId roadSegmentId)
    {
        return runner.Expect([(roadSegmentId, (RoadSegmentProjectionItem?)null)]);
    }
}
