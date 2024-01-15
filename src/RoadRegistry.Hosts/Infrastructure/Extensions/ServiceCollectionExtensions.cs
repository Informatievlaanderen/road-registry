namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using Amazon;
using BackOffice;
using BackOffice.Core;
using BackOffice.FeatureToggles;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoadNetwork.Schema;
using SqlStreamStore;
using Sync.StreetNameRegistry;
using Syndication.Schema;
using System;
using IStreetNameCache = BackOffice.Abstractions.IStreetNameCache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreamStore(this IServiceCollection services)
    {
        return services
            .AddSingleton<IStreamStore>(sp =>
                new MsSqlStreamStoreV3(
                    new MsSqlStreamStoreV3Settings(
                        sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.Events))
                    {
                        Schema = WellKnownSchemas.EventSchema
                    }));
    }

    public static IServiceCollection AddEditorContext(this IServiceCollection services)
    {
        return services
                .AddSingleton(sp => new TraceDbConnection<EditorContext>(
                    new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.EditorProjections)),
                    sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
                .AddSingleton<Func<EditorContext>>(sp =>
                    {
                        var configuration = sp.GetRequiredService<IConfiguration>();
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                        var connectionString = configuration.GetConnectionString(WellKnownConnectionNames.EditorProjections);

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
        services.AddSingleton<IStreetNameCache, StreetNameCache>();

        //syndication
        services
            .AddDbContextFactory<SyndicationContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.SyndicationProjections);
                options
                    .UseSqlServer(connectionString,
                        o => o
                            .EnableRetryOnFailure()
                    );
            })
            .AddSingleton<Func<SyndicationContext>>(sp =>
                () =>
                    new SyndicationContext(
                        new DbContextOptionsBuilder<SyndicationContext>()
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                            .UseSqlServer(
                                sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.SyndicationProjections),
                                options => options
                                    .EnableRetryOnFailure()
                            )
                            .Options)
            )
            .AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                return new TraceDbConnection<SyndicationContext>(
                    new SqlConnection(configuration.GetConnectionString(WellKnownConnectionNames.SyndicationProjections)), configuration["DataDog:ServiceName"]);
            })
            ;

        //kafka
        services
            .AddDbContextFactory<StreetNameConsumerContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.StreetNameConsumerProjections);
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
            .AddSingleton(sp => new TraceDbConnection<RoadNetworkDbContext>(
                new SqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString(WellKnownConnectionNames.Events)),
                sp.GetRequiredService<IConfiguration>()["DataDog:ServiceName"]))
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

    public static IServiceCollection AddOrganizationRepository(this IServiceCollection services)
    {
        return services
            .AddScoped<IOrganizationRepository, OrganizationRepository>();
    }
}


public class DistributedStreamStoreLockOptionsValidator : OptionsValidator<DistributedStreamStoreLockOptions>
{
    protected override void ValidateAndThrowOptions(DistributedStreamStoreLockOptions options)
    {
    }
}
