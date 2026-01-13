namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Handlers.Sqs;

public class FakeSqsQueueConsumer : ISqsQueueConsumer
{
    private readonly JsonSerializer _serializer;

    public FakeSqsQueueConsumer()
    {
        _serializer = JsonSerializer.Create(new FakeSqsOptions().JsonSerializerSettings);
    }

    public async Task<Result<SqsJsonMessage>> Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messages = MemorySqsQueue.GetMessages(queueUrl);
            if (messages.Length == 0)
            {
                break;
            }

            var sqsJsonMessage = messages[0];
            var messageData = sqsJsonMessage.Map(_serializer, SqsJsonMessageAssemblies.Assemblies) ?? throw new ArgumentException("SQS message data is null.");

            await messageHandler(messageData);

            MemorySqsQueue.Consume(queueUrl);
        }

        return Result<SqsJsonMessage>.Success(new SqsJsonMessage());
    }
}
