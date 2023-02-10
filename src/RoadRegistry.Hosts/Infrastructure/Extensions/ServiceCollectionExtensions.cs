namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;

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
}
