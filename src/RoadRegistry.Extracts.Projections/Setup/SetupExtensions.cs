namespace RoadRegistry.Extracts.Projections.Setup;

using Marten;

public static class SetupExtensions
{
    public static StoreOptions ConfigureExtractDocuments(this StoreOptions options)
    {
        RoadNodeExtractProjection.Configure(options);
        RoadSegmentExtractProjection.Configure(options);
        GradeSeparatedJunctionExtractProjection.Configure(options);
        GradeJunctionExtractProjection.Configure(options);
        return options;
    }
}
