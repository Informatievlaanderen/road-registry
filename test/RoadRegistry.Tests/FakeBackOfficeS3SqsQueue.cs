namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Handlers.Sqs;

public class FakeBackOfficeS3SqsQueue : IBackOfficeS3SqsQueue
{
    private readonly JsonSerializer _serializer;

    public FakeBackOfficeS3SqsQueue()
    {
        _serializer = JsonSerializer.Create(new FakeSqsOptions().JsonSerializerSettings);
    }

    public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        var sqsJsonMessage = SqsJsonMessage.Create(message, _serializer);

        MemorySqsQueue.Publish(nameof(FakeBackOfficeS3SqsQueue), sqsJsonMessage);

        return Task.FromResult(true);
    }
}