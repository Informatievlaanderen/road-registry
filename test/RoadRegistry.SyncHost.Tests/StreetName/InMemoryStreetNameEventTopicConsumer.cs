namespace RoadRegistry.SyncHost.Tests.StreetName;
using Sync.StreetNameRegistry;
using SyncHost.StreetName;

public class InMemoryStreetNameEventTopicConsumer : IStreetNameEventTopicConsumer
{
    private readonly Func<StreetNameEventConsumerContext> _dbContextFactory;
    private readonly Queue<object> _messages = new();

    public InMemoryStreetNameEventTopicConsumer(Func<StreetNameEventConsumerContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public InMemoryStreetNameEventTopicConsumer SeedMessage(object value)
    {
        _messages.Enqueue(value);
        return this;
    }

    public async Task ConsumeContinuously(Func<object, StreetNameEventConsumerContext, Task> messageHandler, CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory();

        while (_messages.Any())
        {
            var message = _messages.Dequeue();
            
            await messageHandler(message, dbContext);
        }
    }
}
