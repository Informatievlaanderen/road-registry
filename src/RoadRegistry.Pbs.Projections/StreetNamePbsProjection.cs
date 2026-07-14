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
    public StreetNamePbsProjection(IDbContextFactory<PbsContext> dbContextFactory, ILoggerFactory loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
    {
        When<IEvent<StreetNameWasCreated>>((context, e, ct) =>
            Upsert(context, e.Data.StreetNameId.ToInt32(), e.Data.DutchName, ct));

        When<IEvent<StreetNameWasModified>>((context, e, ct) =>
            Upsert(context, e.Data.StreetNameId.ToInt32(), e.Data.DutchName, ct));

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

    private static async System.Threading.Tasks.Task Upsert(PbsContext context, int id, string naam, System.Threading.CancellationToken ct)
    {
        var record = await context.StreetNameCache.FindAsync([id], ct);
        var isNew = record is null;
        record ??= new StreetNameCacheRecord { StraatnaamId = id };
        record.Naam = naam;
        if (isNew)
        {
            context.StreetNameCache.Add(record);
        }
    }
}
