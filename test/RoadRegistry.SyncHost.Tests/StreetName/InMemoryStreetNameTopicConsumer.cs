namespace RoadRegistry.SyncHost.Tests.StreetName;

using Infrastructure;
using RoadRegistry.StreetName;
using Sync.StreetNameRegistry;

public class InMemoryStreetNameTopicConsumer : IStreetNameTopicConsumer
{
    private readonly Func<StreetNameConsumerContext> _dbContextFactory;
    private readonly Queue<SnapshotMessage> _messages = new();

    public InMemoryStreetNameTopicConsumer(Func<StreetNameConsumerContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public InMemoryStreetNameTopicConsumer SeedMessage(string key, StreetNameSnapshotOsloRecord value)
    {
        _messages.Enqueue(new SnapshotMessage
        {
            Offset = _messages.Count + 1,
            Key = key,
            Value = value
        });
        return this;
    }

    public async Task ConsumeContinuously(Func<SnapshotMessage, StreetNameConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory();

        while (_messages.Any())
        {
            var message = _messages.Dequeue();
            
            await messageHandler(message, dbContext);
        }
    }
}
