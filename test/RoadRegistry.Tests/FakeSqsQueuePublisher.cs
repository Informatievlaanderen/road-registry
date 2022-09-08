namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Exceptions;

public class FakeSqsQueuePublisher : ISqsQueuePublisher
{
    private readonly ILogger<FakeSqsQueuePublisher> _logger;
    private readonly SqsOptions _sqsOptions;
    private object _latestMessage;

    public FakeSqsQueuePublisher(
        SqsOptions sqsOptions,
        ILogger<FakeSqsQueuePublisher> logger)
    {
        _sqsOptions = sqsOptions ?? throw new SqsOptionsNotFoundException(nameof(sqsOptions));
        _logger = logger ?? throw new LoggerNotFoundException<FakeSqsQueuePublisher>();
    }

    public T GetLatestMessage<T>()
    {
        return (T)_latestMessage;
    }

    public virtual async Task<bool> CopyToQueue<T>(string queueName, T message, SqsQueueOptions sqsQueueOptions, CancellationToken cancellationToken) where T : class
    {
        _latestMessage = message;
        return await Task.FromResult(true);
    }
}
