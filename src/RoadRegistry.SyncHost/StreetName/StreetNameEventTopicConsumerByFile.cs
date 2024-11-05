namespace RoadRegistry.SyncHost.StreetName;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Sync.StreetNameRegistry;
using RoadRegistry.SyncHost.Infrastructure;

internal class StreetNameEventTopicConsumerByFile : KafkaTopicConsumerByFile<StreetNameEventConsumerContext>, IStreetNameEventTopicConsumer
{
    public StreetNameEventTopicConsumerByFile(
        IDbContextFactory<StreetNameEventConsumerContext> dbContextFactory,
        string path,
        ILogger<StreetNameEventTopicConsumerByFile> logger
    ) : base(dbContextFactory, path, new GrarContractsMessageSerializer(EventsJsonSerializerSettingsProvider.CreateSerializerSettings()), logger)
    {
    }

    public Task ConsumeContinuously(Func<object, StreetNameEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        return ConsumeTopicContinuously(messageHandler, cancellationToken);
    }
}
