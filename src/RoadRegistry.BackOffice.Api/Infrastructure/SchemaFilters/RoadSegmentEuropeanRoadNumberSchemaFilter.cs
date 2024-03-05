namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

public class RoadSegmentEuropeanRoadNumberSchemaFilter : EnumSchemaFilter<EuropeanRoadNumber>
{
    public RoadSegmentEuropeanRoadNumberSchemaFilter()
        : base(EuropeanRoadNumber.All)
    {
    }
}
