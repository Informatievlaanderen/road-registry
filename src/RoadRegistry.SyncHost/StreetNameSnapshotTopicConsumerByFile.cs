namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreetName;
using Sync.StreetNameRegistry;
using System;
using System.Threading;
using System.Threading.Tasks;

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
