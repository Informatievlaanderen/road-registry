namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentSurfaceTypeSchemaFilter : EnumSchemaFilter<RoadSegmentSurfaceType>
{
    public RoadSegmentSurfaceTypeSchemaFilter()
        : base(RoadSegmentSurfaceType.All)
    {
    }
}
