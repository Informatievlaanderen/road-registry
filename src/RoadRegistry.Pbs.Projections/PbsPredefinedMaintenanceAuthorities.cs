namespace RoadRegistry.Pbs.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice;
using Schema;
using Schema.Records;

// "andere" (-7) and "niet gekend" (-8) are maintenance authorities a road segment can carry without an organization
// behind them, so no organization event ever puts them in the Wegbeheerder code list. They are seeded instead, by the
// organization projection when it is initialized - a new read model, or one a rebuild emptied - which writes them in
// the same transaction as the projection-state row of its first batch. Otherwise that projection leaves these two rows
// alone.
//
// That initialization is the only thing that seeds them, and it has to stay the only thing. PbsCodeListSyncService did
// it too, for a read model projected before these rows existed, and the two ran at the same moment: the code list sync
// and the projection daemon are started together, so both read an empty code list, both inserted (-7) and (-8) from
// their own transaction, and whichever committed second got a primary key violation. On a new database that was the
// projection's first batch, which paused the shard over two constant rows. A read model that is missing them now needs
// a rebuild, which empties it and initializes the projection again.
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
