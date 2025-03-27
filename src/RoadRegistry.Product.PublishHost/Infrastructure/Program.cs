namespace RoadRegistry.Product.PublishHost.Infrastructure;

using System;
using System.Threading.Tasks;
using Autofac;
using Azure.Identity;
using Azure.Storage.Blobs;
using BackOffice.Extensions;
using BackOffice.Framework;
using CloudStorageClients;
using Configurations;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Hosts.Infrastructure.Modules;
using HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
                        .AddProductContext()

                        .Configure<AzureBlobOptions>(hostContext.Configuration.GetSection(nameof(AzureBlobOptions)))
                        .AddAzureBlobServiceClient()
                        .AddSingleton<AzureBlobClient>()

                        .Configure<MetadataCenterOptions>(hostContext.Configuration.GetSection(nameof(MetadataCenterOptions)))
                        .AddSingleton<ITokenProvider, TokenProvider>()
                        .AddTransient<MetaDataCenterHttpClient>()

                        .AddSingleton<ProductPublisher>();
                }
            )
            .ConfigureContainer((_, builder) =>
            {
                builder.RegisterModule<BlobClientModule>();
            })
            .ConfigureRunCommand(async (sp, stoppingToken) =>
            {
                var extractRequestCleanup = sp.GetRequiredService<ProductPublisher>();

                await extractRequestCleanup.ExecuteAsync(stoppingToken);
            })
            .Build();

        await roadRegistryHost
            .Log((sp, logger) => {
                logger.LogKnownSqlServerConnectionStrings(roadRegistryHost.Configuration);

                var blobClientOptions = sp.GetRequiredService<RoadRegistry.BackOffice.Configuration.BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync();
    }
}

internal static class InternalServiceCollectionExtensions
{
    public static IServiceCollection AddAzureBlobServiceClient(this IServiceCollection services)
    {
        return services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureBlobOptions>>().Value;

            if (options.IsAzurite)
            {
                return new BlobServiceClient(
                    options.ConnectionString,
                    new BlobClientOptions(BlobClientOptions.ServiceVersion.V2020_04_08));
            }

            return new BlobServiceClient(
                new Uri(options.BaseUrl),
                new ClientSecretCredential(options.TenantId, options.ClientKey, options.ClientSecret),
                new BlobClientOptions(BlobClientOptions.ServiceVersion.V2020_04_08));
        });
    }
}
