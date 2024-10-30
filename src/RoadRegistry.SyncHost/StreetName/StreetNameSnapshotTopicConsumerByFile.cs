namespace RoadRegistry.SyncHost.StreetName;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.StreetName;
using RoadRegistry.Sync.StreetNameRegistry;
using RoadRegistry.SyncHost.Infrastructure;

internal class StreetNameSnapshotTopicConsumerByFile : KafkaTopicConsumerByFile<StreetNameSnapshotConsumerContext>, IStreetNameSnapshotTopicConsumer
{
    public StreetNameSnapshotTopicConsumerByFile(
        IDbContextFactory<StreetNameSnapshotConsumerContext> dbContextFactory,
        string path,
        ILogger<StreetNameSnapshotTopicConsumerByFile> logger
    ) : base(dbContextFactory, path, new SnapshotMessageSerializer<StreetNameSnapshotRecord>(EventsJsonSerializerSettingsProvider.CreateSerializerSettings()), logger)
    {
    }

    public Task ConsumeContinuously(Func<SnapshotMessage, StreetNameSnapshotConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        return ConsumeTopicContinuously((message, context) => messageHandler((SnapshotMessage)message, context), cancellationToken);
    }
}
