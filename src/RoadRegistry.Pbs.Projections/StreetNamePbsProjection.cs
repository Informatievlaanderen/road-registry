namespace RoadRegistry.Pbs.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

// RoadSegmentStreetNameAttributes -> internal StreetNameCache (id -> Dutch name), used to fill STRTNM / LSTRNM / RSTRNM labels.
public class StreetNamePbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public StreetNamePbsProjection()
    {
        When<IEvent<StreetNameWasCreated>>((context, e, ct) =>
            Insert(context, e.Data.StreetNameId.ToInt32(), e.Data.DutchName, ct));

        When<IEvent<StreetNameWasModified>>((context, e, ct) =>
            Update(context, e.Data.StreetNameId.ToInt32(), e.Data.DutchName, ct));

        When<IEvent<StreetNameWasRemoved>>(async (context, e, ct) =>
        {
            var record = await context.StreetNameCache.FindAsync([e.Data.StreetNameId.ToInt32()], ct);
            if (record is not null)
            {
                context.StreetNameCache.Remove(record);
            }
        });

        // A rename merges the street name into a destination; the old id no longer stands on its own. Affected road
        // segments get their own RoadSegmentStreetNameIdWasChanged event, so here we just drop the stale cache entry.
        When<IEvent<StreetNameWasRenamed>>(async (context, e, ct) =>
        {
            var record = await context.StreetNameCache.FindAsync([e.Data.StreetNameId.ToInt32()], ct);
            if (record is not null)
            {
                context.StreetNameCache.Remove(record);
            }
        });
    }

    // Upsert, not insert. An event that creates a cache entry is not guaranteed to arrive once per key: a replay of
    // the correlation re-delivers it, and the event stream itself can carry an import and a create for the same id.
    // A bare Add turned either of those into a primary key violation that paused the shard - which is how
    // RoadNetworkChangesWmsWfsV2Projection fell over on OrganisatieCache. FindAsync also sees what this batch has
    // already added, so a duplicate inside one SaveChanges resolves here too.
    private static async System.Threading.Tasks.Task Insert(PbsContext context, int id, string? naam, System.Threading.CancellationToken ct)
    {
        var cache = await context.StreetNameCache.FindAsync([id], ct);
        if (cache is null)
        {
            context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = id, Naam = naam });
        }
        else
        {
            cache.Naam = naam;
        }
    }

    private static async System.Threading.Tasks.Task Update(PbsContext context, int id, string? naam, System.Threading.CancellationToken ct)
    {
        var record = await context.StreetNameCache.FindAsync([id], ct);
        if (record is null)
        {
            return;
        }
        record.Naam = naam;
    }
}
