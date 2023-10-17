namespace RoadRegistry.Projector.Infrastructure.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContext<TDbContext>(this IServiceCollection services, string connectionStringName)
        where TDbContext : DbContext
    {
        return services.AddDbContext<TDbContext>((sp, options) => options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .UseNetTopologySuite()));
    }
}
