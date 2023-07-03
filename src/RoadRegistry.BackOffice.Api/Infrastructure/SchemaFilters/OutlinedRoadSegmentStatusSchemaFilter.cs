namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentStatusSchemaFilter : EnumSchemaFilter<RoadSegmentStatus.Edit, RoadSegmentStatus>
{
    public OutlinedRoadSegmentStatusSchemaFilter()
        : base(RoadSegmentStatus.Edit.Editable)
    {
    }
}
