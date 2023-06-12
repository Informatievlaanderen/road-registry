namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentSurfaceTypeSchemaFilter : EnumSchemaFilter<RoadSegmentSurfaceType.Edit, RoadSegmentSurfaceType>
{
    public OutlinedRoadSegmentSurfaceTypeSchemaFilter()
        : base(RoadSegmentSurfaceType.Edit.Editable)
    {
    }
}
