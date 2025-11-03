namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections;

using RoadRegistry.Infrastructure.MartenDb.Projections;

public static class ProjectionExtensions
{
    public static Task Expect(this MartenProjectionTestRunner runner, params RoadNodeProjectionItem[] expectedDocuments)
    {
        return runner.Expect(expectedDocuments.Select(x => (x.Id, (RoadNodeProjectionItem?)x)).ToArray());
    }

    public static Task Expect(this MartenProjectionTestRunner runner, params RoadSegmentProjectionItem[] expectedDocuments)
    {
        return runner.Expect(expectedDocuments.Select(x => (x.Id, (RoadSegmentProjectionItem?)x)).ToArray());
    }
}
