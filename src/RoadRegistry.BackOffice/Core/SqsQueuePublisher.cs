namespace RoadRegistry.BackOffice;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Exceptions;
using Microsoft.Extensions.Logging;
using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

public class SqsQueuePublisher : ISqsQueuePublisher
{
    private readonly ILogger<SqsQueuePublisher> _logger;
    private readonly SqsOptions _sqsOptions;

    public SqsQueuePublisher(
        SqsOptions sqsOptions,
        ILogger<SqsQueuePublisher> logger)
    {
        _sqsOptions = sqsOptions ?? throw new SqsOptionsNotFoundException(nameof(sqsOptions));
        _logger = logger ?? throw new LoggerNotFoundException<SqsQueuePublisher>();
    }

    public virtual async Task<bool> CopyToQueue<T>(string queueName, T message, SqsQueueOptions sqsQueueOptions, CancellationToken cancellationToken) where T : class
    {
        var result = await Sqs.CopyToQueue(_sqsOptions, queueName, message, sqsQueueOptions, cancellationToken);
        _logger.LogTrace("Placed message onto queue {QueueName}: {Message}", queueName, JsonSerializer.Serialize(message));
        return result;
    }
}
