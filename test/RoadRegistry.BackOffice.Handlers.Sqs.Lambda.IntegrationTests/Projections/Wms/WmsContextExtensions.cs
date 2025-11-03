namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections.Wms;

using RoadRegistry.Wms.Schema;

public static class WmsContextExtensions
{
    public static Task Expect(this MartenProjectionTestRunner runner, RoadSegmentRecord[] expectedDocuments)
    {
        return runner.Expect(expectedDocuments.Select(x => (x.Id, (RoadSegmentRecord?)x)).ToArray());
    }
}
