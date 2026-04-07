namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

using RoadRegistry.BackOffice.Api.Inwinning;

public class InwinningsstatusSchemaFilter : EnumSchemaFilter<Inwinningsstatus>
{
    public InwinningsstatusSchemaFilter()
        : base(Inwinningsstatus.All)
    {
    }
}
