namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentAccessRestrictionSchemaFilter : EnumSchemaFilter<RoadSegmentAccessRestriction>
{
    public RoadSegmentAccessRestrictionSchemaFilter()
        : base(RoadSegmentAccessRestriction.All)
    {
    }
}
