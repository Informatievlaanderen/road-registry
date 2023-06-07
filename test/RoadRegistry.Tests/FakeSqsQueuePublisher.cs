namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;

public class FakeSqsQueuePublisher : ISqsQueuePublisher
{
    private readonly JsonSerializer _serializer;

    public FakeSqsQueuePublisher()
    {
        _serializer = JsonSerializer.Create(new FakeSqsOptions().JsonSerializerSettings);
    }

    public virtual async Task<bool> CopyToQueue<T>(string queueUrl, T message, SqsQueueOptions sqsQueueOptions, CancellationToken cancellationToken) where T : class
    {
        var sqsJsonMessage = SqsJsonMessage.Create(message, _serializer);

        MemorySqsQueue.Publish(queueUrl, sqsJsonMessage);

        return await Task.FromResult(true);
    }
}