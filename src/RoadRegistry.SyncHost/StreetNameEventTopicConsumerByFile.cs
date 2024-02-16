namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sync.StreetNameRegistry;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class StreetNameEventTopicConsumerByFile : KafkaTopicConsumerByFile<StreetNameEventConsumerContext>, IStreetNameEventTopicConsumer
{
    public StreetNameEventTopicConsumerByFile(
        IDbContextFactory<StreetNameEventConsumerContext> dbContextFactory,
        string path,
        ILogger<StreetNameEventTopicConsumerByFile> logger
    ) : base(dbContextFactory, path, new JsonMessageSerializer(EventsJsonSerializerSettingsProvider.CreateSerializerSettings()), logger)
    {
    }

    public Task ConsumeContinuously(Func<object, StreetNameEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        return ConsumeTopicContinuously(messageHandler, cancellationToken);
    }
}
