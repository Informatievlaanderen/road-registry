namespace RoadRegistry.Read.Projections.Setup;

using Marten;

public static class SetupExtensions
{
    public static void ConfigureReadDocuments(this StoreOptions options)
    {
        OrganizationReadProjection.Configure(options);
        RoadNodeReadProjection.Configure(options);
        RoadSegmentReadProjection.Configure(options);
        GradeSeparatedJunctionReadProjection.Configure(options);
        GradeJunctionReadProjection.Configure(options);
    }
}
