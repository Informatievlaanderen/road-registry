namespace RoadRegistry.BackOffice.Api.Infrastructure.SchemaFilters;

using System.Linq;
using RoadRegistry.ValueObjects;

// Every road node type an API reader can be handed, which is not quite every type there is: 'schijnknoop' exists for
// the extracts and never reaches an edit or read endpoint, so listing it would document a value nobody can get back.
public class RoadNodeTypeV2SchemaFilter : EnumSchemaFilter<RoadNodeTypeV2>
{
    public RoadNodeTypeV2SchemaFilter()
        : base(RoadNodeTypeV2.All.Where(x => x != RoadNodeTypeV2.Schijnknoop))
    {
    }
}
