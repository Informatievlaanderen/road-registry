namespace RoadRegistry.BackOffice;

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
}
