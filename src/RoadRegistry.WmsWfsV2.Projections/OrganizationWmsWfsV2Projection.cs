namespace RoadRegistry.WmsWfsV2.Projections;

using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class OrganizationWmsWfsV2Projection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public OrganizationWmsWfsV2Projection()
    {
        When<IEvent<OrganizationWasImported>>((context, e, ct) =>
            Insert(context, e.Data.OrganizationId.ToString(), e.Data.Name, null, ct));

        When<IEvent<OrganizationWasCreated>>((context, e, ct) =>
            Insert(context, e.Data.OrganizationId.ToString(), e.Data.Name, e.Data.OvoCode, ct));

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
        });
    }

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
}
