namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Exceptions;
using Microsoft.Extensions.Logging;

public class SqsQueueConsumer : ISqsQueueConsumer
{
    private readonly ILogger<SqsQueueConsumer> _logger;
    private readonly SqsOptions _sqsOptions;

    public SqsQueueConsumer(SqsOptions sqsOptions, ILogger<SqsQueueConsumer> logger)
    {
        _sqsOptions = sqsOptions ?? throw new SqsOptionsNotFoundException(nameof(sqsOptions));
        _logger = logger ?? throw new LoggerNotFoundException<SqsQueuePublisher>();
    }

    public Task Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken)
        => SqsConsumer.Consume(_sqsOptions, queueUrl, messageHandler, cancellationToken);
}
