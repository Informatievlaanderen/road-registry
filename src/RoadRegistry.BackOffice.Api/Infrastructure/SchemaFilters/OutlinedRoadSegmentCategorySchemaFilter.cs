namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class OutlinedRoadSegmentCategorySchemaFilter : EnumSchemaFilter<RoadSegmentCategory.Edit, RoadSegmentCategory>
{
    public OutlinedRoadSegmentCategorySchemaFilter()
        : base(RoadSegmentCategory.Edit.Editable)
    {
    }
}
