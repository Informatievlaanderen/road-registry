namespace RoadRegistry.BackOffice.ExtractHost;

using System;
using System.Threading.Tasks;
using Abstractions;
using Autofac;
using Configuration;
using Editor.Schema;
using Extensions;
using Extracts;
using FeatureToggles;
using Framework;
using Handlers.Extracts;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using SqlStreamStore;
using ZipArchiveWriters.ExtractHost;
using ZipArchiveWriters.ExtractHost.V1;
using ZipArchiveWriters.ExtractHost.V2;

public class Program
{
    public const int HostingPort = 10003;
    private static readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.BackOffice);

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((_, services) =>
            {
                services
                    .RegisterOptions<ZipArchiveWriterOptions>()
                    .AddTicketing()
                    .AddEmailClient()
                    .AddRoadRegistrySnapshot()
                    .AddRoadNetworkEventWriter()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddSingleton<IEventProcessorPositionStore>(sp =>
                        new SqlEventProcessorPositionStore(
                            new SqlConnectionStringBuilder(
                                sp.GetService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.ExtractHost)
                            ),
                            WellKnownSchemas.ExtractHostSchema))
                    .AddSingleton<IZipArchiveWriter>(sp =>
                        sp.GetRequiredService<UseNetTopologySuiteForExtractFeatureToggle>().FeatureEnabled
                            ? new RoadNetworkExtractZipArchiveWriter(
                                sp.GetService<ZipArchiveWriterOptions>(),
                                sp.GetService<IStreetNameCache>(),
                                sp.GetService<RecyclableMemoryStreamManager>(),
                                sp.GetRequiredService<FileEncoding>(),
                                sp.GetRequiredService<ILogger<RoadNetworkExtractZipArchiveWriter>>()
                            )
                            : new RoadNetworkExtractToZipArchiveWriter(
                                sp.GetService<ZipArchiveWriterOptions>(),
                                sp.GetService<IStreetNameCache>(),
                                sp.GetService<RecyclableMemoryStreamManager>(),
                                sp.GetRequiredService<FileEncoding>(),
                                sp.GetRequiredService<ILogger<RoadNetworkExtractToZipArchiveWriter>>()
                            )
                    )
                    .AddSingleton<IRoadNetworkExtractArchiveAssembler>(sp =>
                        new RoadNetworkExtractArchiveAssembler(
                            sp.GetService<RecyclableMemoryStreamManager>(),
                            sp.GetService<Func<EditorContext>>(),
                            sp.GetService<IZipArchiveWriter>()))
                    .AddEditorContext()
                    .AddOrganizationCache()
                    .AddStreetNameCache()
                    .AddFeatureCompare()
                    .AddSingleton(sp => new EventHandlerModule[]
                    {
                        new RoadNetworkExtractEventModule(
                            sp.GetService<ILifetimeScope>(),
                            sp.GetService<RoadNetworkExtractDownloadsBlobClient>(),
                            sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                            sp.GetService<IRoadNetworkExtractArchiveAssembler>(),
                            sp.GetService<IStreamStore>(),
                            ApplicationMetadata,
                            sp.GetService<IRoadNetworkEventWriter>(),
                            sp.GetService<IExtractUploadFailedEmailClient>(),
                            sp.GetService<ILogger<RoadNetworkExtractEventModule>>())
                    })
                    .AddSingleton(sp => AcceptStreamMessage.WhenEqualToMessageType(sp.GetRequiredService<EventHandlerModule[]>(), PositionStoreEventProcessor.EventMapping))
                    .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(sp.GetRequiredService<EventHandlerModule[]>())))
                    .AddHostedService<EventProcessor>()
                    .AddHostedService(sp =>
                    {
                        var modules = new EventHandlerModule[]
                        {
                            new ExtractHostHealthModule(
                                sp.GetService<ILifetimeScope>(),
                                sp.GetService<RoadNetworkExtractDownloadsBlobClient>(),
                                sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                                sp.GetService<ILoggerFactory>())
                        };

                        var acceptStreamMessageFilter = AcceptStreamMessage.WhenEqualToMessageType(modules, PositionStoreEventProcessor.EventMapping);
                        var eventHandlerDispatcher = Dispatch.Using(Resolve.WhenEqualToMessage(modules));

                        return new HealthEventProcessor(
                            sp.GetRequiredService<IHostApplicationLifetime>(),
                            sp.GetRequiredService<IStreamStore>(),
                            sp.GetRequiredService<IEventProcessorPositionStore>(),
                            acceptStreamMessageFilter,
                            eventHandlerDispatcher,
                            sp.GetRequiredService<Scheduler>(),
                            sp.GetRequiredService<ILoggerFactory>()
                        );
                    });
            })
            .ConfigureHealthChecks(HostingPort, builder => builder
                .AddHostedServicesStatus()
            )
            .ConfigureContainer((_, builder) =>
            {
                builder
                    .RegisterModule<ContextModule>();
            })
            .Build();

        await roadRegistryHost
            .LogSqlServerConnectionStrings([
                WellKnownConnectionNames.Events,
                WellKnownConnectionNames.ExtractHost,
                WellKnownConnectionNames.ExtractHostAdmin,
                WellKnownConnectionNames.EditorProjections
            ])
            .Log((sp, logger) =>
            {
                var blobClientOptions = sp.GetService<BlobClientOptions>();
                logger.LogBlobClientCredentials(blobClientOptions);
            })
            .RunAsync(async (_, _, configuration) => { await new SqlEventProcessorPositionStoreSchema(new SqlConnectionStringBuilder(configuration.GetRequiredConnectionString(WellKnownConnectionNames.ExtractHostAdmin))).CreateSchemaIfNotExists(WellKnownSchemas.ExtractHostSchema).ConfigureAwait(false); });
    }
}
