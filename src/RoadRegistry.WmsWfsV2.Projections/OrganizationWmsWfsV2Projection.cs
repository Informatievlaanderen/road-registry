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
    public OrganizationWmsWfsV2Projection(IDbContextFactory<WmsWfsV2Context> dbContextFactory, ILoggerFactory? loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
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
        });
    }

    // Upsert the organization cache (all organizations). A null argument leaves the existing cached value unchanged, so
    // events that only carry part of the organization state keep the rest intact.
    private static async Task Upsert(WmsWfsV2Context context, string organisatieId, string? name, string? ovoCode, bool? isMaintainer, CancellationToken ct)
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
    }
}
