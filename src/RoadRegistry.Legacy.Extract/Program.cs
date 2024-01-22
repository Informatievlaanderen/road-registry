namespace RoadRegistry.Legacy.Extract;

using Amazon.S3;
using Autofac;
using BackOffice;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Readers;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Infrastructure;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddSingleton<WellKnownBinaryReader>()
                .AddSingleton<IEventReader, LegacyEventReader>()
                .AddSingleton<LegacyStreamArchiveWriter>()
                .AddSingleton(
                    new SqlConnection(
                        hostContext.Configuration.GetConnectionString(WellKnownConnectionNames.Legacy)
                    )
                ))
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .Register(c => new S3BlobClient(c.Resolve<AmazonS3Client>(), c.Resolve<S3BlobClientOptions>().Buckets[WellKnownBuckets.ImportLegacyBucket]))
                    .As<IBlobClient>().SingleInstance();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellKnownConnectionNames.Legacy
            })
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                var blobClient = sp.GetRequiredService<IBlobClient>();
                var reader = sp.GetRequiredService<IEventReader>();
                var writer = sp.GetRequiredService<LegacyStreamArchiveWriter>();
                var logger = sp.GetRequiredService<ILogger<Program>>();

                await OptimizeDatabasePerformance(
                    new SqlConnectionStringBuilder(
                        configuration.GetConnectionString(WellKnownConnectionNames.Legacy)), logger);

                await blobClient.ProvisionResources(host);

                await using var connection = sp.GetRequiredService<SqlConnection>();
                await connection.OpenAsync();
                await writer.WriteAsync(reader.ReadEvents(connection));
            });
    }

    private static async Task OptimizeDatabasePerformance(SqlConnectionStringBuilder builder,
        ILogger<Program> logger, CancellationToken token = default)
    {
        logger.LogInformation("Optimizing database for performance ...");
        using (var connection = new SqlConnection(builder.ConnectionString))
        {
            await connection.OpenAsync(token).ConfigureAwait(false);
            using (var command = new SqlCommand(@"
IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE name='IX_WB_WS' AND object_id = OBJECT_ID('dbo.wegbreedte')) BEGIN CREATE INDEX [IX_WB_WS] ON [dbo].[wegbreedte] ([wegsegmentID]) END
IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE name='IX_RS_WS' AND object_id = OBJECT_ID('dbo.rijstroken')) BEGIN CREATE INDEX [IX_RS_WS] ON [dbo].[rijstroken] ([wegsegmentID]) END
IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE name='IX_WV_WS' AND object_id = OBJECT_ID('dbo.wegverharding')) BEGIN CREATE INDEX [IX_WV_WS] ON [dbo].[wegverharding] ([wegsegmentID]) END", connection))
            {
                command.CommandType = CommandType.Text;
                await command.ExecuteNonQueryAsync(token);
            }
        }
    }

}
