namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Autofac;
using Core;
using Extensions;
using Extracts;
using Framework;
using Handlers.Sqs;
using Hosts;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Snapshot.Handlers;
using Snapshot.Handlers.Sqs;
using SqlStreamStore;
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
                .AddHostedService<RoadNetworkCommandProcessor>()
                .AddHostedService<RoadNetworkExtractCommandProcessor>()
                .AddTicketing()
                .AddEmailClient(hostContext.Configuration)
                .AddRoadRegistrySnapshot()
                .AddRoadNetworkEventWriter()
                .AddScoped(_ => new EventSourcedEntityMap())
                .AddSingleton<ICommandProcessorPositionStore>(sp =>
                    new SqlCommandProcessorPositionStore(
                        new SqlConnectionStringBuilder(
                            sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.CommandHost)
                        ),
                        WellknownSchemas.CommandHostSchema))
                .AddDistributedStreamStoreLockOptions())
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage(new CommandHandlerModule[] {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveBeforeFeatureCompareValidator(sp.GetRequiredService<FileEncoding>()),
                    new ZipArchiveAfterFeatureCompareValidator(sp.GetRequiredService<FileEncoding>()),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetRequiredService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveBeforeFeatureCompareValidator(sp.GetRequiredService<FileEncoding>()),
                    new ZipArchiveAfterFeatureCompareValidator(sp.GetRequiredService<FileEncoding>()),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkSnapshotCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<IMediator>(),
                    sp.GetRequiredService<ILifetimeScope>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotWriter>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                )
            }))
            .ConfigureContainer((context, builder) =>
            {
                builder
                    .RegisterModule<RoadRegistry.Snapshot.Handlers.Sqs.MediatorModule>()
                    .RegisterModule<SqsHandlersModule>()
                    .RegisterModule<SnapshotSqsHandlersModule>();
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
