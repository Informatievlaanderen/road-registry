namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using System;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamStore(this IServiceCollection services)
    {
        return services
            .AddSingleton<IStreamStore>(sp =>
                new MsSqlStreamStoreV3(
                    new MsSqlStreamStoreV3Settings(
                        sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.Events))
                    {
                        Schema = WellknownSchemas.EventSchema
                    }));
    }

    public static IServiceCollection AddEditorContext(this IServiceCollection services)
    {
        return services
                .AddSingleton(sp => new TraceDbConnection<EditorContext>(
                    new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.EditorProjections)),
                    sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                .AddSingleton<Func<EditorContext>>(sp =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                        var connectionString = configuration.GetConnectionString(WellknownConnectionNames.EditorProjections);

                        return () =>
                            new EditorContext(
                                new DbContextOptionsBuilder<EditorContext>()
                                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                    .UseLoggerFactory(loggerFactory)
                                    .UseSqlServer(connectionString, options =>
                                        options
                                            .UseNetTopologySuite()
                                    ).Options);
                    }
                )
            ;
    }
}
