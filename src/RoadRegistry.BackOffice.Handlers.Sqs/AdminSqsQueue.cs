namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Configuration;

public interface IAdminSqsQueue : ISqsQueue
{
}

public class AdminSqsQueue : IAdminSqsQueue
{
    private readonly ISqsQueue _queue;

    public AdminSqsQueue(ISqsQueueFactory sqsQueueFactory, SqsQueueUrlOptions sqsQueueUrlOptions)
    {
        _queue = sqsQueueFactory.Create(sqsQueueUrlOptions.Admin);
    }

    public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        return _queue.Copy(message, queueOptions, cancellationToken);
    }
}
