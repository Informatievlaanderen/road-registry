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
                .AddHostedService<RoadNetworkCommandProcessor>()
                .AddHostedService<RoadNetworkExtractCommandProcessor>()
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
                    sp.GetService<IRoadNetworkSnapshotWriter>(),
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
