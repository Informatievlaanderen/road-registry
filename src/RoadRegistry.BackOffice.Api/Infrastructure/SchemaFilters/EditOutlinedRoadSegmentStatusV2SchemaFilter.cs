namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class EditOutlinedRoadSegmentStatusV2SchemaFilter : EnumSchemaFilter<RoadSegmentStatusV2.EditOutlined, RoadSegmentStatusV2>
{
    public EditOutlinedRoadSegmentStatusV2SchemaFilter()
        : base(RoadSegmentStatusV2.EditOutlined.Values)
    {
    }
}
