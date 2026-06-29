namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentStatusV2SchemaFilter : EnumSchemaFilter<RoadSegmentStatusV2.Edit, RoadSegmentStatusV2>
{
    public OutlinedRoadSegmentStatusV2SchemaFilter()
        : base(RoadSegmentStatusV2.Edit.Outlined)
    {
    }
}
