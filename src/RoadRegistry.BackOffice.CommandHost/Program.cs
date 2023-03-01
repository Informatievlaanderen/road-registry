namespace RoadRegistry.BackOffice.CommandHost;

using Abstractions;
using Core;
using Extracts;
using Framework;
using Hosts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Snapshot.Handlers;
using SqlStreamStore;
using System;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Extensions;
using FeatureToggles;
using Handlers.Sqs;
using MediatR;
using Snapshot.Handlers.Sqs;
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
                .AddRoadRegistrySnapshot()
                .AddSingleton<ICommandProcessorPositionStore>(sp =>
                    new SqlCommandProcessorPositionStore(
                        new SqlConnectionStringBuilder(
                            sp.GetService<IConfiguration>().GetConnectionString(WellknownConnectionNames.CommandHost)
                        ),
                        WellknownSchemas.CommandHostSchema))
                .AddDistributedStreamStoreLockOptions()
                .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap()))
            .ConfigureCommandDispatcher(sp => Resolve.WhenEqualToMessage(new CommandHandlerModule[] {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetRequiredService<RoadNetworkUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<Func<EventSourcedEntityMap>>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<Func<EventSourcedEntityMap>>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetRequiredService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<Func<EventSourcedEntityMap>>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    new ZipArchiveAfterFeatureCompareValidator(Encoding.GetEncoding(1252)),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>()
                ),
                new RoadNetworkSnapshotCommandModule(
                    sp.GetRequiredService<IStreamStore>(),
                    sp.GetRequiredService<IMediator>(),
                    sp.GetRequiredService<Func<EventSourcedEntityMap>>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotReader>(),
                    sp.GetRequiredService<IRoadNetworkSnapshotWriter>(),
                    sp.GetRequiredService<IClock>(),
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.GetRequiredService<UseSnapshotSqsRequestFeatureToggle>()
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
