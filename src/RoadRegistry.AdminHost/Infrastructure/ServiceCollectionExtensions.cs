namespace RoadRegistry.AdminHost.Infrastructure;

using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
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
            .AddSingleton(sp => new TraceDbConnection<ProductContext>(
                new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.EditorProjections)),
                sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
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
