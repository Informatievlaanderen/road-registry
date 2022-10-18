namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice;

public class FakeSqsQueueConsumer : ISqsQueueConsumer
{
    public async Task Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messages = FakeSqsQueue.GetMessages(queueUrl);
            if (messages.Length == 0) break;

            var sqsJsonMessage = messages[0];
            var messageData = sqsJsonMessage.Map() ?? throw new ArgumentException("SQS message data is null.");

            await messageHandler(messageData);

            FakeSqsQueue.Consume(queueUrl);
        }
    }
}
