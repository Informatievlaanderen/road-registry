namespace RoadRegistry.AdminHost.Infrastructure;

using BackOffice;
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
            .AddDbContext<ProductContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.ProductProjections),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite())
            );
    }
}
