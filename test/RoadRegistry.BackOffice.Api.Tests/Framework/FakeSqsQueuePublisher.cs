namespace RoadRegistry.BackOffice.Api.Tests.Framework;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.Logging;

public class FakeSqsQueuePublisher : SqsQueuePublisher
{
    public FakeSqsQueuePublisher(SqsOptions sqsOptions, ILogger<SqsQueuePublisher> logger) : base(sqsOptions, logger)
    {
    }

    public override Task<bool> CopyToQueue<T>(string queueName, T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
        => Task.FromResult(true);
}
