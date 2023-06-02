namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentSurfaceTypeSchemaFilter : EnumSchemaFilter<RoadSegmentSurfaceType.Outlined, RoadSegmentSurfaceType>
{
    public OutlinedRoadSegmentSurfaceTypeSchemaFilter()
        : base(RoadSegmentSurfaceType.Outlined.AllOutlined)
    {
    }
}
