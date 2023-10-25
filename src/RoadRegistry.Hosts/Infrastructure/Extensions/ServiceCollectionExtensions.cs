namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using System.Configuration;
using Amazon;
using BackOffice;
using BackOffice.FeatureToggles;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using StreetNameConsumer.Schema;
using Syndication.Schema;
using IStreetNameCache = BackOffice.Abstractions.IStreetNameCache;

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
        services.AddSingleton<IStreetNameCache>(sp =>
        {
            var featureToggle = sp.GetRequiredService<UseKafkaStreetNameCacheFeatureToggle>();
            if (featureToggle.FeatureEnabled)
            {
                return sp.GetRequiredService<StreetNameConsumer.Projections.StreetNameCache>();
            }

            return sp.GetRequiredService<Syndication.Projections.StreetNameCache>();
        });

        //syndication
        services
            .AddDbContextFactory<SyndicationContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.SyndicationProjections);
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
                                sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                options => options
                                    .EnableRetryOnFailure()
                            )
                            .Options)
            )
            .AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                return new TraceDbConnection<SyndicationContext>(
                    new SqlConnection(configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections)), configuration["DataDog:ServiceName"]);
            })
            .AddSingleton<Syndication.Projections.StreetNameCache>()
            ;

        //kafka
        services
            .AddDbContextFactory<StreetNameConsumerContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(WellknownConnectionNames.StreetNameConsumerProjections);
                options
                    .UseSqlServer(connectionString,
                        o => o
                            .EnableRetryOnFailure()
                    );
            })
            .AddSingleton<StreetNameConsumer.Projections.StreetNameCache>()
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
