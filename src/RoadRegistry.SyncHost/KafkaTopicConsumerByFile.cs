namespace RoadRegistry.SyncHost;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal abstract class KafkaTopicConsumerByFile<TDbContext>
    where TDbContext : ConsumerDbContext<TDbContext>
{
    private readonly IMessageSerializer<string> _messageSerializer;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly string _path;
    private readonly ILogger _logger;

    protected KafkaTopicConsumerByFile(
        IDbContextFactory<TDbContext> dbContextFactory,
        string path,
        IMessageSerializer<string> messageSerializer,
        ILogger logger
    )
    {
        _dbContextFactory = dbContextFactory.ThrowIfNull();
        _path = path.ThrowIfNull();
        _messageSerializer = messageSerializer.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    protected async Task ConsumeTopicContinuously(Func<object, TDbContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(_path))
                {
                    var messages = JsonConvert.DeserializeObject<JsonKafkaMessage[]>(await File.ReadAllTextAsync(_path, cancellationToken), EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
                    if (messages is not null)
                    {
                        foreach (var message in messages)
                        {
                            var messageData = _messageSerializer.Deserialize(message.Value, new MessageContext
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

                            await messageHandler(messageData, dbContext);

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

    private sealed class JsonKafkaMessage
    {
        public Dictionary<string, string> Headers { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public long Offset { get; set; }
    }
}
