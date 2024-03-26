namespace RoadRegistry.Snapshot.Handlers.Sqs;

using BackOffice.Configuration;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;

public class SnapshotSqsQueue : ISqsQueue
{
    private readonly ISqsQueue _queue;

    public SnapshotSqsQueue(ISqsQueueFactory sqsQueueFactory, SqsQueueUrlOptions sqsQueueUrlOptions)
    {
        _queue = sqsQueueFactory.Create(sqsQueueUrlOptions.Snapshot);
    }

    public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        return _queue.Copy(message, queueOptions, cancellationToken);
    }
}
