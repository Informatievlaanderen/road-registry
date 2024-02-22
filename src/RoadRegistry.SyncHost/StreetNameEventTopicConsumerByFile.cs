namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Infrastructure;
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
    ) : base(dbContextFactory, path, new GrarContractsMessageSerializer(EventsJsonSerializerSettingsProvider.CreateSerializerSettings()), logger)
    {
    }

    public Task ConsumeContinuously(Func<object, StreetNameEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        return ConsumeTopicContinuously(messageHandler, cancellationToken);
    }
}
