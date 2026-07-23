namespace RoadRegistry.WmsWfsV2.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class StreetNameWmsWfsV2Projection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public StreetNameWmsWfsV2Projection()
    {
        When<IEvent<StreetNameWasCreated>>((context, e, ct) =>
            Insert(context, e.Data.StreetNameId.ToInt32(), e.Data.DutchName, ct));

        When<IEvent<StreetNameWasModified>>(async (context, e, ct) =>
        {
            var id = e.Data.StreetNameId.ToInt32();
            await Update(context, id, e.Data.DutchName, ct);
            await RefreshStreetNameLabels(context, id, e.Data.DutchName, ct);
        });

        When<IEvent<StreetNameWasRemoved>>((context, e, ct) =>
            Remove(context, e.Data.StreetNameId.ToInt32(), ct));

        When<IEvent<StreetNameWasRenamed>>((context, e, ct) =>
            Remove(context, e.Data.StreetNameId.ToInt32(), ct));
    }

    private static Task Insert(WmsWfsV2Context context, int id, string? naam, CancellationToken ct)
    {
        context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = id, Naam = naam });
        return Task.CompletedTask;
    }

    private static async Task Update(WmsWfsV2Context context, int id, string? naam, CancellationToken ct)
    {
        var record = await context.StreetNameCache.FindAsync([id], ct);
        if (record is null)
        {
            return;
        }
        record.Naam = naam;
    }

    private static async Task Remove(WmsWfsV2Context context, int id, CancellationToken ct)
    {
        var record = await context.StreetNameCache.FindAsync([id], ct);
        if (record is not null)
        {
            context.StreetNameCache.Remove(record);
        }
    }

    // Recompute LSTRNM / RSTRNM / STRNM on every derived row referencing this street name. The changed name is applied
    // directly (its cache row is mutated but not yet saved); the opposite side's name is read from the cache.
    private static async Task RefreshStreetNameLabels(WmsWfsV2Context context, int streetNameId, string? newName, CancellationToken ct)
    {
        var rows = await context.DerivedRoadSegments
            .Where(x => x.LSTRNMID == streetNameId || x.RSTRNMID == streetNameId)
            .ToListAsync(ct);
        if (rows.Count == 0)
        {
            return;
        }

        var otherIds = rows
            .SelectMany(r => new[] { r.LSTRNMID, r.RSTRNMID })
            .Where(x => x is not null && x.Value != streetNameId)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
        var otherNames = otherIds.Count == 0
            ? new Dictionary<int, string?>()
            : await context.StreetNameCache.Where(x => otherIds.Contains(x.StraatnaamId)).ToDictionaryAsync(x => x.StraatnaamId, x => x.Naam, ct);

        string? NameFor(int? id) => id is null
            ? null
            : id.Value == streetNameId ? newName : otherNames.TryGetValue(id.Value, out var n) ? n : null;

        foreach (var r in rows)
        {
            var lName = NameFor(r.LSTRNMID);
            var rName = NameFor(r.RSTRNMID);
            r.LSTRNM = lName;
            r.RSTRNM = rName;
            r.STRNM = WmsWfsV2DerivedLabels.BuildStreetNameLabel(r.LSTRNMID, r.RSTRNMID, lName, rName);
        }
    }
}
