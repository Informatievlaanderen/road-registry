namespace RoadRegistry.Pbs.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice;
using Schema;
using Schema.Records;

// "andere" (-7) and "niet gekend" (-8) are maintenance authorities a road segment can carry without an organization
// behind them, so no organization event ever puts them in the Wegbeheerder code list. They are seeded instead: when the
// organization projection is initialized - a new read model, or one a rebuild emptied - and by PbsCodeListSyncService at
// startup, for a read model that was projected before these rows existed. Otherwise the organization projection leaves
// these two rows alone.
public static class PbsPredefinedMaintenanceAuthorities
{
    public static bool Contains(string? organisatieId)
    {
        return OrganizationName.PredefinedTranslations.All.Any(x => x.Identifier.ToString() == organisatieId);
    }

    // Adds the rows that are missing and corrects the ones that differ; saving is up to the caller.
    public static async Task SyncAsync(PbsContext context, CancellationToken cancellationToken)
    {
        foreach (var translation in OrganizationName.PredefinedTranslations.All)
        {
            var beheer = translation.Identifier.ToString();
            var label = translation.Name.ToString();

            var codeList = await context.RoadSegmentMaintenanceAuthorityCodeList.FindAsync([beheer], cancellationToken);
            if (codeList is null)
            {
                context.RoadSegmentMaintenanceAuthorityCodeList.Add(new RoadSegmentMaintenanceAuthorityCodeListRecord
                {
                    BEHEER = beheer,
                    LBLBEHEER = label,
                    OVOCODE = null
                });
            }
            else if (codeList.LBLBEHEER != label || codeList.OVOCODE is not null)
            {
                codeList.LBLBEHEER = label;
                codeList.OVOCODE = null;
                context.RoadSegmentMaintenanceAuthorityCodeList.Update(codeList);
            }
        }
    }
}
