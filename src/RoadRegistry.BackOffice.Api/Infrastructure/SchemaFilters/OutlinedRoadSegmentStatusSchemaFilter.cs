namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentStatusSchemaFilter : EnumSchemaFilter<RoadSegmentStatus.Outlined, RoadSegmentStatus>
{
    public OutlinedRoadSegmentStatusSchemaFilter()
        : base(RoadSegmentStatus.Outlined.All)
    {
    }
}
