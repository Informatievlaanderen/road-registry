namespace RoadRegistry.Extracts.Projections.Setup;

using Marten;

public static class SetupExtensions
{
    public static void ConfigureExtractDocuments(this StoreOptions options)
    {
        RoadNodeProjection.Configure(options);
        RoadSegmentProjection.Configure(options);
        GradeSeparatedJunctionProjection.Configure(options);
    }
}
