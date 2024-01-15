namespace RoadRegistry.AdminHost.Infrastructure;

using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Hosts.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Product.Schema;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductContext(this IServiceCollection services)
    {
        return services
            .AddTraceDbConnection<ProductContext>(WellKnownConnectionNames.ProductProjections, ServiceLifetime.Scoped)
            .AddDbContext<ProductContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<TraceDbConnection<ProductContext>>(),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite())
            );
    }
}
