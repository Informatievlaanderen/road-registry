namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentCategorySchemaFilter : EnumSchemaFilter<RoadSegmentCategory>
{
    public RoadSegmentCategorySchemaFilter()
        : base(RoadSegmentCategory.All)
    {
    }
}
