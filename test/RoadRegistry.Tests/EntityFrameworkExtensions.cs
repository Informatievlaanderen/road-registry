namespace Microsoft.EntityFrameworkCore;

using Extensions.DependencyInjection;
using Extensions.Logging;
using RoadRegistry.BackOffice;

public static class EntityFrameworkExtensions
{
    public static IQueryable<TEntity> IgnoreQueryFilters<TEntity>(
        this IQueryable<TEntity> source, bool ignoreQueryFilters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);

        if (ignoreQueryFilters)
        {
            return source.IgnoreQueryFilters();
        }

        return source;
    }

    public static IServiceCollection AddInMemoryDbContext<TDbContext>(this IServiceCollection services, ServiceLifetime lifetimeScope = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return services
            .AddDbContext<TDbContext>(InMemoryDbContextOptions);
    }

    public static IServiceCollection AddInMemoryDbContextOptionsBuilder<TDbContext>(this IServiceCollection services, ServiceLifetime lifetimeScope = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return services
            .AddSingleton<ConfigureDbContextOptionsBuilder<TDbContext>>(InMemoryDbContextOptions);
    }

    private static void InMemoryDbContextOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
    }
}
