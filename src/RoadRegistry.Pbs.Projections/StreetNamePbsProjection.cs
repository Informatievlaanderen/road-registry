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

    private static System.Threading.Tasks.Task Insert(PbsContext context, int id, string? naam, System.Threading.CancellationToken ct)
    {
        context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = id, Naam = naam });
        return System.Threading.Tasks.Task.CompletedTask;
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
