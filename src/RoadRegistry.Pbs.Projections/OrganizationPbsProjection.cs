namespace RoadRegistry.Pbs.Projections;

using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class OrganizationPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public OrganizationPbsProjection()
    {
        When<IEvent<OrganizationWasImported>>((context, e, ct) =>
            Upsert(context, e.Data.OrganizationId.ToString(), e.Data.Name, null, null, ct));

        When<IEvent<OrganizationWasCreated>>((context, e, ct) =>
            Upsert(context, e.Data.OrganizationId.ToString(), e.Data.Name, e.Data.OvoCode, null, ct));

        When<IEvent<OrganizationWasModified>>((context, e, ct) =>
            Upsert(context, e.Data.OrganizationId.ToString(), e.Data.Name, e.Data.OvoCode, e.Data.IsMaintainer, ct));

        When<IEvent<OrganizationWasRemoved>>(async (context, e, ct) =>
        {
            var organisatieId = e.Data.OrganizationId.ToString();

            var cache = await context.OrganizationCache.FindAsync([organisatieId], ct);
            if (cache is not null)
            {
                context.OrganizationCache.Remove(cache);
            }

            var codeList = await context.RoadSegmentMaintenanceAuthorityCodeList.FindAsync([organisatieId], ct);
            if (codeList is not null)
            {
                context.RoadSegmentMaintenanceAuthorityCodeList.Remove(codeList);
            }
        });
    }

    // Upsert the organization cache (all organizations) and reconcile the wegbeheerder code list. A null argument leaves
    // the existing cached value unchanged, so events that only carry part of the organization state keep the rest intact.
    private static async Task Upsert(PbsContext context, string organisatieId, string? name, string? ovoCode, bool? isMaintainer, CancellationToken ct)
    {
        var cache = await context.OrganizationCache.FindAsync([organisatieId], ct);
        var isNew = cache is null;
        cache ??= new OrganizationCacheRecord { OrganisatieId = organisatieId };
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
        if (isNew)
        {
            context.OrganizationCache.Add(cache);
        }

        await SyncCodeList(context, cache, ct);
    }

    // The code list only holds the organizations that are a road maintainer; keep it in sync with the cache.
    private static async Task SyncCodeList(PbsContext context, OrganizationCacheRecord cache, CancellationToken ct)
    {
        var codeList = await context.RoadSegmentMaintenanceAuthorityCodeList.FindAsync([cache.OrganisatieId], ct);

        if (cache.IsWegbeheerder)
        {
            var isNew = codeList is null;
            codeList ??= new RoadSegmentMaintenanceAuthorityCodeListRecord { BEHEER = cache.OrganisatieId };
            codeList.LBLBEHEER = cache.Naam;
            codeList.OVOCODE = cache.OvoCode;
            if (isNew)
            {
                context.RoadSegmentMaintenanceAuthorityCodeList.Add(codeList);
            }
        }
        else if (codeList is not null)
        {
            context.RoadSegmentMaintenanceAuthorityCodeList.Remove(codeList);
        }
    }
}
