namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.Extensions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StreetName;
using Sync.StreetNameRegistry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal class StreetNameTopicConsumerByFile : IStreetNameSnapshotTopicConsumer
{
    private readonly IDbContextFactory<StreetNameSnapshotConsumerContext> _dbContextFactory;
    private readonly string _path;
    private readonly ILogger<StreetNameTopicConsumerByFile> _logger;

    public StreetNameTopicConsumerByFile(
        IDbContextFactory<StreetNameSnapshotConsumerContext> dbContextFactory,
        string path,
        ILogger<StreetNameTopicConsumerByFile> logger
    )
    {
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _path = path.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public async Task ConsumeContinuously(Func<SnapshotMessage, StreetNameSnapshotConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        var jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var messageSerializer = new SnapshotMessageSerializer<StreetNameSnapshotRecord>(jsonSerializerSettings);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(_path))
                {
                    var messages = JsonConvert.DeserializeObject<SnapshotMessageInJson[]>(await File.ReadAllTextAsync(_path, cancellationToken), jsonSerializerSettings);
                    if (messages is not null)
                    {
                        foreach (var message in messages)
                        {
                            var messageData = messageSerializer.Deserialize(message.Value, new MessageContext
                            {
                                Key = new MessageKey(message.Key),
                                Offset = new Offset(message.Offset)
                            });

                            var idempotenceKey = message.Headers.TryGetValue(MessageHeader.IdempotenceKey, out var idempotenceHeaderValue)
                                ? idempotenceHeaderValue
                                : message.Value.ToSha512();

                            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

                            var messageAlreadyProcessed = await dbContext.ProcessedMessages
                                .AsNoTracking()
                                .AnyAsync(x => x.IdempotenceKey == idempotenceKey, cancellationToken)
                                .ConfigureAwait(false);

                            if (messageAlreadyProcessed)
                            {
                                continue;
                            }

                            var processedMessage = new ProcessedMessage(idempotenceKey, DateTimeOffset.Now);

                            await dbContext.ProcessedMessages
                                .AddAsync(processedMessage, cancellationToken)
                                .ConfigureAwait(false);

                            await messageHandler((SnapshotMessage)messageData, dbContext);

                            await dbContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("File does not exist: {Path}", _path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error consuming kafka events by file");
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}

public class SnapshotMessageInJson
{
    public Dictionary<string, string> Headers { get; set; }
    public string Value { get; set; }
    public string Key { get; set; }
    public long Offset { get; set; }
}
