namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Framework;
using SqlStreamStore;
using SqlStreamStore.Streams;

public static class StreamStoreExtensions
{
    public static async Task<TMessage> GetLastMessage<TMessage>(this IStreamStore store)
        where TMessage : class
    {
        var page = await store.ReadAllBackwards(Position.End, 1);
        var message = page.Messages.Single();
        Assert.Equal(typeof(TMessage).Name, message.Type);

        return JsonConvert.DeserializeObject<TMessage>(await message.GetJsonData());
    }

    public static async Task<TMessage> GetLastMessageIfTypeIs<TMessage>(this IStreamStore store)
        where TMessage : class
    {
        var page = await store.ReadAllBackwards(Position.End, 1);
        var message = page.Messages.Single();
        if (typeof(TMessage).Name == message.Type)
        {
            return JsonConvert.DeserializeObject<TMessage>(await message.GetJsonData());
        }

        return default;
    }

    public static Task<long> Given(this IStreamStore store, EventMapping mapping, JsonSerializerSettings jsonSerializerSettings, StreamNameConverter converter, StreamName streamName, params object[] events)
    {
        var givens = events.Select(@event => new RecordedEvent(streamName, @event)).ToArray();
        return Given(store, mapping, jsonSerializerSettings, converter, givens);
    }

    public static async Task<long> Given(this IStreamStore store, EventMapping mapping, JsonSerializerSettings jsonSerializerSettings, StreamNameConverter converter, RecordedEvent[] givens)
    {
        var checkpoint = Position.Start;
        foreach (var stream in givens.GroupBy(given => given.Stream))
        {
            var result = await store.AppendToStream(
                converter(new StreamName(stream.Key)).ToString(),
                ExpectedVersion.NoStream,
                stream.Select((given, index) => new NewStreamMessage(
                    Deterministic.Create(Deterministic.Namespaces.Events,
                        $"{given.Stream}-{index}"),
                    mapping.GetEventName(given.Event.GetType()),
                    JsonConvert.SerializeObject(given.Event, jsonSerializerSettings)
                )).ToArray());
            checkpoint = result.CurrentPosition + 1;
        }

        return checkpoint;
    }
}
