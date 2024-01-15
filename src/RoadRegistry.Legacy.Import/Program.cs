namespace RoadRegistry.Legacy.Import;

using Amazon.S3;
using Autofac;
using BackOffice;
using BackOffice.Configuration;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BackOffice.Infrastructure;
using Be.Vlaanderen.Basisregisters.BlobStore;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddSingleton(
                    new SqlConnection(
                        hostContext.Configuration.GetConnectionString(WellKnownConnectionNames.Events)
                    )
                )
                .AddSingleton(new LegacyStreamArchiveReader(
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                        DateParseHandling = DateParseHandling.DateTime,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    }
                ))
                .AddSingleton<LegacyStreamEventsWriter>())
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .Register(c => new S3BlobClient(c.Resolve<AmazonS3Client>(), c.Resolve<S3BlobClientOptions>().Buckets[WellKnownBuckets.ImportLegacyBucket]))
                    .As<IBlobClient>().SingleInstance();
            })
            .Build();

        await roadRegistryHost.RunAsync();
    }
}
