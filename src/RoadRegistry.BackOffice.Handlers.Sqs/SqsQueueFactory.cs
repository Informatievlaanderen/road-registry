namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using RoadRegistry.Extensions;
using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

public interface ISqsQueueFactory
{
    ISqsQueue Create(string queueUrl);
}

public class SqsQueueFactory: ISqsQueueFactory
{
    private readonly SqsOptions _sqsOptions;

    public SqsQueueFactory(SqsOptions sqsOptions)
    {
        _sqsOptions = sqsOptions.ThrowIfNull();
    }

    public ISqsQueue Create(string queueUrl)
    {
        return new SqsQueue(_sqsOptions, queueUrl);
    }
}
