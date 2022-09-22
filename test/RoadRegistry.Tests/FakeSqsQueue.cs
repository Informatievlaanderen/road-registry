namespace RoadRegistry.Tests
{
    using System.Collections.Concurrent;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

    internal class FakeSqsQueue
    {
        private static readonly ConcurrentDictionary<string, Queue<SqsJsonMessage>> Queues = new();

        public static void Publish(string queueName, SqsJsonMessage message)
        {
            var queue = Queues.GetOrAdd(queueName, name => new Queue<SqsJsonMessage>());
            queue.Enqueue(message);
        }

        public static SqsJsonMessage[] GetMessages(string queueUrl)
        {
            var queueName = SqsQueue.ParseQueueNameFromQueueUrl(queueUrl);

            if (Queues.TryGetValue(queueName, out Queue<SqsJsonMessage> queue))
            {
                return queue.ToArray();
            }

            return Array.Empty<SqsJsonMessage>();
        }

        public static SqsJsonMessage Consume(string queueUrl)
        {
            var queueName = SqsQueue.ParseQueueNameFromQueueUrl(queueUrl);

            if (Queues.TryGetValue(queueName, out Queue<SqsJsonMessage> queue))
            {
                return queue.Dequeue();
            }

            return default;
        }
    }
}
