namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using Amazon;
using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoadNetwork.Schema;
using SqlStreamStore;
using Sync.StreetNameRegistry;
using System;
using System.Linq;
using BackOffice.Extensions;
using IStreetNameCache = BackOffice.IStreetNameCache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamStore(this IServiceCollection services)
    {
        return services
            .AddSingleton<IStreamStore>(sp =>
                new MsSqlStreamStoreV3(
                    new MsSqlStreamStoreV3Settings(
                        sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.Events))
                    {
                        Schema = WellKnownSchemas.EventSchema
                    }));
    }

    public static IServiceCollection AddEditorContext(this IServiceCollection services)
    {
        return services
                .AddTraceDbConnection<EditorContext>(WellKnownConnectionNames.EditorProjections, ServiceLifetime.Singleton)
                .AddSingleton<Func<EditorContext>>(sp =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                        var connectionString = configuration.GetRequiredConnectionString(WellKnownConnectionNames.EditorProjections);

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
                .AddDbContext<EditorContext>((sp, options) => options
                    .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseSqlServer(
                        sp.GetRequiredService<TraceDbConnection<EditorContext>>(),
                        sqlOptions => sqlOptions
                            .UseNetTopologySuite())
                )
            ;
    }

    public static IServiceCollection AddDistributedStreamStoreLockOptions(this IServiceCollection services)
    {
        services.AddSingleton<DistributedStreamStoreLockOptionsValidator>();
        
        return services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var config = configuration.GetOptions<DistributedStreamStoreLockConfiguration>();

            return new DistributedStreamStoreLockOptions
            {
                Region = RegionEndpoint.GetBySystemName(config.Region),
                AwsAccessKeyId = config.AccessKeyId,
                AwsSecretAccessKey = config.AccessKeySecret,
                TableName = config.TableName,
                LeasePeriod = TimeSpan.FromMinutes(config.LeasePeriodInMinutes),
                ThrowOnFailedRenew = config.ThrowOnFailedRenew,
                TerminateApplicationOnFailedRenew = config.TerminateApplicationOnFailedRenew,
                ThrowOnFailedAcquire = config.ThrowOnFailedAcquire,
                TerminateApplicationOnFailedAcquire = config.TerminateApplicationOnFailedAcquire,
                Enabled = config.Enabled,
                AcquireLockRetryDelaySeconds = config.AcquireLockRetryDelaySeconds
            };
        });
    }

    public static IServiceCollection AddStreetNameCache(this IServiceCollection services)
    {
        services
            .AddSingleton<IStreetNameCache, StreetNameCache>()
            .AddDbContextFactory<StreetNameSnapshotProjectionContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.StreetNameProjections);
                options
                    .UseSqlServer(connectionString,
                        o => o
                            .EnableRetryOnFailure()
                    );
            })
            .AddDbContextFactory<StreetNameEventProjectionContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.StreetNameProjections);
                options
                    .UseSqlServer(connectionString,
                        o => o
                            .EnableRetryOnFailure()
                    );
            })
            ;

        return services;
    }

    public static IServiceCollection AddRoadNetworkDbIdGenerator(this IServiceCollection services)
    {
        return services
            .AddTraceDbConnection<RoadNetworkDbContext>(WellKnownConnectionNames.Events, ServiceLifetime.Singleton)
            .AddDbContext<RoadNetworkDbContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(
                    sp.GetRequiredService<TraceDbConnection<RoadNetworkDbContext>>(),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite()
                        .MigrationsHistoryTable("__EFMigrationsHistory", RoadNetworkDbContext.Schema)
                )
            )
            .AddScoped<IRoadNetworkIdGenerator, RoadNetworkDbIdGenerator>();
    }

    public static IServiceCollection AddOrganizationCache(this IServiceCollection services)
    {
        return services
            .AddScoped<IOrganizationCache, OrganizationCache>();
    }

    public static IServiceCollection AddDbContextEventProcessorServices<TDbContextEventProcessor, TDbContext>(this IServiceCollection services,
        Func<IServiceProvider, ConnectedProjection<TDbContext>[]> projections)
        where TDbContextEventProcessor : DbContextEventProcessor<TDbContext>
        where TDbContext: RunnerDbContext<TDbContext>
    {
        services
            .AddSingleton(sp => new DbContextEventProcessorProjections<TDbContextEventProcessor, TDbContext>(projections(sp).Where(x => x is not null).ToArray()))
            ;

        return services;
    }
}


public class DistributedStreamStoreLockOptionsValidator : OptionsValidator<DistributedStreamStoreLockOptions>
{
    protected override void ValidateAndThrowOptions(DistributedStreamStoreLockOptions options)
    {
    }
}
