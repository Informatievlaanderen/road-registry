namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Configuration;
using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

public interface IAdminSqsQueue : ISqsQueue
{
}

public class AdminSqsQueue : IAdminSqsQueue
{
    private readonly ISqsQueue _queue;

    public AdminSqsQueue(SqsOptions sqsOptions, SqsQueueUrlOptions sqsQueueUrlOptions)
    {
        _queue = new SqsQueue(sqsOptions, sqsQueueUrlOptions.Admin);
    }

    public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        return _queue.Copy(message, queueOptions, cancellationToken);
    }
}
