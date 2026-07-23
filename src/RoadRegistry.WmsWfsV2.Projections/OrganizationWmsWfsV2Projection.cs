namespace RoadRegistry.WmsWfsV2.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

// OrganisatieCache (id -> name) resolves the maintainer labels. The derived-segment LBLBEHEER category depends on the
// organization name, so a rename/removal refreshes that column on every segment maintained by the organization.
public class OrganizationWmsWfsV2Projection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public OrganizationWmsWfsV2Projection()
    {
        // Created events: the organization does not exist yet, so insert directly without a lookup. Segments referencing
        // it are derived afterwards, so there is nothing to refresh here.
        When<IEvent<OrganizationWasImported>>((context, e, ct) =>
            Insert(context, e.Data.OrganizationId.ToString(), e.Data.Name, null, ct));

        When<IEvent<OrganizationWasCreated>>((context, e, ct) =>
            Insert(context, e.Data.OrganizationId.ToString(), e.Data.Name, e.Data.OvoCode, ct));

        // Modify: update the cache and refresh the maintainer label on the segments it maintains (the name feeds the
        // LBLBEHEER category).
        When<IEvent<OrganizationWasModified>>(async (context, e, ct) =>
        {
            var id = e.Data.OrganizationId.ToString();
            await Update(context, id, e.Data.Name, e.Data.OvoCode, e.Data.IsMaintainer, ct);
            await RefreshBeheerLabels(context, id, e.Data.Name, ct);
        });

        When<IEvent<OrganizationWasRemoved>>(async (context, e, ct) =>
        {
            var organisatieId = e.Data.OrganizationId.ToString();

            var cache = await context.OrganizationCache.FindAsync([organisatieId], ct);
            if (cache is not null)
            {
                context.OrganizationCache.Remove(cache);
            }

            await RefreshBeheerLabels(context, organisatieId, null, ct);
        });
    }

    // A created organization: insert the cache row directly, without a lookup.
    private static Task Insert(WmsWfsV2Context context, string organisatieId, string? name, string? ovoCode, CancellationToken ct)
    {
        context.OrganizationCache.Add(new OrganizationCacheRecord
        {
            OrganisatieId = organisatieId,
            Naam = name,
            OvoCode = ovoCode
        });
        return Task.CompletedTask;
    }

    // A modify: update the existing organization cache. A null argument leaves the existing cached value unchanged, so
    // events that only carry part of the organization state keep the rest intact.
    private static async Task Update(WmsWfsV2Context context, string organisatieId, string? name, string? ovoCode, bool? isMaintainer, CancellationToken ct)
    {
        var cache = await context.OrganizationCache.FindAsync([organisatieId], ct);
        if (cache is null)
        {
            return;
        }
        if (name is not null)
        {
            cache.Naam = name;
        }
        if (ovoCode is not null)
        {
            cache.OvoCode = ovoCode;
        }
        if (isMaintainer is not null)
        {
            cache.IsWegbeheerder = isMaintainer.Value;
        }
    }

    // Recompute the maintainer name (LBLLBEHEER/LBLRBEHEER) and category (LBLBEHEER) on every derived row maintained by
    // this organization. The changed name is applied directly (its cache row is mutated but not yet saved); the opposite
    // side's organization name is read from the cache.
    private static async Task RefreshBeheerLabels(WmsWfsV2Context context, string organisatieId, string? newName, CancellationToken ct)
    {
        var rows = await context.DerivedRoadSegments
            .Where(x => x.LBEHEER == organisatieId || x.RBEHEER == organisatieId)
            .ToListAsync(ct);
        if (rows.Count == 0)
        {
            return;
        }

        var otherIds = rows
            .SelectMany(r => new[] { r.LBEHEER, r.RBEHEER })
            .Where(x => x is not null && x != organisatieId)
            .Select(x => x!)
            .Distinct()
            .ToList();
        var otherNames = otherIds.Count == 0
            ? new Dictionary<string, string?>()
            : await context.OrganizationCache.Where(x => otherIds.Contains(x.OrganisatieId)).ToDictionaryAsync(x => x.OrganisatieId!, x => x.Naam, ct);

        string? NameFor(string? id) => id is null
            ? null
            : id == organisatieId ? newName : otherNames.TryGetValue(id, out var n) ? n : null;

        foreach (var r in rows)
        {
            var lOrg = NameFor(r.LBEHEER);
            var rOrg = NameFor(r.RBEHEER);
            r.LBLLBEHEER = lOrg;
            r.LBLRBEHEER = rOrg;
            r.LBLBEHEER = WmsWfsV2DerivedLabels.LblBeheer(r.STATUS, r.LBEHEER, r.RBEHEER, lOrg, rOrg);
        }
    }
}
