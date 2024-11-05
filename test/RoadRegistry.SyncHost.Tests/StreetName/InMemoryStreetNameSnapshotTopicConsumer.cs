namespace RoadRegistry.SyncHost.Tests.StreetName;

using Infrastructure;
using RoadRegistry.StreetName;
using Sync.StreetNameRegistry;
using SyncHost.StreetName;

public class InMemoryStreetNameSnapshotTopicConsumer : IStreetNameSnapshotTopicConsumer
{
    private readonly Func<StreetNameSnapshotConsumerContext> _dbContextFactory;
    private readonly Queue<SnapshotMessage> _messages = new();

    public InMemoryStreetNameSnapshotTopicConsumer(Func<StreetNameSnapshotConsumerContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public InMemoryStreetNameSnapshotTopicConsumer SeedMessage(string key, StreetNameSnapshotRecord value)
    {
        _messages.Enqueue(new SnapshotMessage
        {
            Offset = _messages.Count + 1,
            Key = key,
            Value = value
        });
        return this;
    }

    public async Task ConsumeContinuously(Func<SnapshotMessage, StreetNameSnapshotConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory();

        while (_messages.Any())
        {
            var message = _messages.Dequeue();
            
            await messageHandler(message, dbContext);
        }
    }
}
