namespace RoadRegistry.Snapshot.Handlers.Sqs;

using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

public class SnapshotSqsQueue : ISqsQueue
{
    private readonly ISqsQueue _queue;

    public SnapshotSqsQueue(SqsOptions sqsOptions, SqsQueueUrlOptions sqsQueueUrlOptions)
    {
        _queue = new SqsQueue(sqsOptions, sqsQueueUrlOptions.Snapshot);
    }

    public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        return _queue.Copy(message, queueOptions, cancellationToken);
    }
}
