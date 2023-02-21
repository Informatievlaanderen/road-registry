namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using Amazon;
using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using Amazon.S3;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOptions<TOptions>(this IServiceCollection services)
        where TOptions : class, new()
    {
        var configurationSectionName = new TOptions() is IHasConfigurationKey hasConfigurationKey ? hasConfigurationKey.GetConfigurationKey() : null;
        return RegisterOptions<TOptions>(services, configurationSectionName);
    }

    public static IServiceCollection RegisterOptions<TOptions>(this IServiceCollection services, string configurationSectionKey)
        where TOptions : class, new()
    {
        return services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var options = new TOptions();
            if (configurationSectionKey != null)
            {
                configuration.GetSection(configurationSectionKey).Bind(options);
            }
            else
            {
                configuration.Bind(options);
            }

            return options;
        });
    }

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

    public static IServiceCollection AddDistributedStreamStoreLockOptions(this IServiceCollection services)
    {
        return services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var config = new DistributedStreamStoreLockConfiguration();
            configuration.GetSection(DistributedStreamStoreLockConfiguration.SectionName).Bind(config);

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
                Enabled = config.Enabled
            };
        });
    }

    public static IServiceCollection AddDistributedS3Cache(this IServiceCollection services)
    {
        //TODO-rik send to yusuf
        return services
            .RegisterOptions<DistributedS3CacheOptions>(nameof(DistributedS3CacheOptions))
            .AddSingleton<DistributedS3Cache>()
            .AddTransient<S3CacheService>();
    }
}
