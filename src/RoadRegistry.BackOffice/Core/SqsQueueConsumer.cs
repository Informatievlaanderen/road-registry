namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SqsQueueConsumer : ISqsQueueConsumer
{
    private readonly ILogger<SqsQueueConsumer> _logger;
    private readonly SqsOptions _sqsOptions;

    public SqsQueueConsumer(SqsOptions sqsOptions, ILogger<SqsQueueConsumer> logger)
    {
        _sqsOptions = sqsOptions ?? throw new SqsOptionsNotFoundException(nameof(sqsOptions));
        _logger = logger ?? throw new LoggerNotFoundException<SqsQueueConsumer>();
    }

    public async Task<Result<SqsJsonMessage>> Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken)
    {
        return await SqsConsumer.Consume(_sqsOptions, queueUrl, message =>
        {
            _logger.LogTrace("Dequeued message from queue {QueueName}: {Message}", queueUrl, JsonConvert.SerializeObject(message, _sqsOptions.JsonSerializerSettings));
            return messageHandler.Invoke(message);
        }, cancellationToken);
    }
}
