namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Infrastructure;

using Abstractions;
using Abstractions.Configuration;
using Autofac;
using Configuration;
using Consumers;
using Core;
using Extensions;
using Extracts;
using Framework;
using Handlers.Sqs;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using System;
using System.Text;
using System.Threading.Tasks;
using Uploads;
using ZipArchiveWriters.Validation;

public class Program
{
    protected Program()
    {
    }
    
    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) => services
                .AddHostedService<FeatureCompareMessageConsumer>()
                .RegisterOptions<FeatureCompareMessagingOptions>()
                .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap()))
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage(new CommandHandlerModule[] {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()

                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }))
            .ConfigureContainer((hostContext, builder) =>
            {
                builder.RegisterModule(new MediatorModule());
                builder.RegisterModule(new SqsHandlersModule());
                builder.RegisterModule(new Handlers.Sqs.MediatorModule());
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings(new[]
            {
                WellknownConnectionNames.Events,
                WellknownConnectionNames.CommandHost,
                WellknownConnectionNames.CommandHostAdmin,
                WellknownConnectionNames.Snapshots
            })
            .Log((sp, logger) => {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (sp, host, configuration) =>
            {
                await
                    new SqlCommandProcessorPositionStoreSchema(
                        new SqlConnectionStringBuilder(
                            configuration.GetConnectionString(WellknownConnectionNames.CommandHostAdmin))
                    ).CreateSchemaIfNotExists(WellknownSchemas.CommandHostSchema).ConfigureAwait(false);
            });
    }
}
