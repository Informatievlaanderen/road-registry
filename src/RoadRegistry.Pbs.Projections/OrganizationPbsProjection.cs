namespace RoadRegistry.Pbs.Projections;

using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using RoadRegistry.BackOffice;
using RoadRegistry.Extensions;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class OrganizationPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public OrganizationPbsProjection()
    {
        When<IEvent<OrganizationWasImported>>((context, e, ct) =>
            Insert(context, e.Data.OrganizationId.ToString(), e.Data.Name, null, ct));

        When<IEvent<OrganizationWasCreated>>((context, e, ct) =>
            Insert(context, e.Data.OrganizationId.ToString(), e.Data.Name, e.Data.OvoCode, ct));

        // Modify: update the existing organization; nothing to insert.
        When<IEvent<OrganizationWasModified>>((context, e, ct) =>
            Update(context, e.Data.OrganizationId.ToString(), e.Data.Name, e.Data.OvoCode, e.Data.IsMaintainer, ct));

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

    private static Task Insert(PbsContext context, string organisatieId, string? name, string? ovoCode, CancellationToken ct)
    {
        context.OrganizationCache.Add(new OrganizationCacheRecord
        {
            OrganisatieId = organisatieId,
            Naam = name?.WithMaxLength(OrganizationName.MaxLength),
            OvoCode = ovoCode
        });
        return Task.CompletedTask;
    }

    private static async Task Update(PbsContext context, string organisatieId, string? name, string? ovoCode, bool? isMaintainer, CancellationToken ct)
    {
        var cache = await context.OrganizationCache.FindAsync([organisatieId], ct);
        if (cache is null)
        {
            return;
        }
        if (name is not null)
        {
            cache.Naam = name.WithMaxLength(OrganizationName.MaxLength);
        }
        if (ovoCode is not null)
        {
            cache.OvoCode = ovoCode;
        }
        if (isMaintainer is not null)
        {
            cache.IsWegbeheerder = isMaintainer.Value;
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
