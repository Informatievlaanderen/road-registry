namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using Amazon;
using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoadRegistry.Editor.Schema;
using SqlStreamStore;
using System;
using FluentValidation;
using FluentValidation.Results;

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

    public static IServiceCollection AddDistributedStreamStoreLockOptions(this IServiceCollection services)
    {
        services.AddSingleton<DistributedStreamStoreLockOptionsValidator>();

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
}


public class DistributedStreamStoreLockOptionsValidator : OptionsValidator<DistributedStreamStoreLockOptions>
{
    protected override void ValidateAndThrowOptions(DistributedStreamStoreLockOptions options)
    {
    }
}
