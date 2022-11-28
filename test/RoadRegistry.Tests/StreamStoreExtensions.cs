namespace RoadRegistry.Tests;

using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using Xunit;

public static class StreamStoreExtensions
{
    public static async Task<TCommand> GetLastCommand<TCommand>(this IStreamStore store)
        where TCommand : class
    {
        var page = await store.ReadAllBackwards(Position.End, 1);
        var message = page.Messages.Single();
        Assert.Equal(typeof(TCommand).Name, message.Type);

        return JsonConvert.DeserializeObject<TCommand>(await message.GetJsonData());
    }
}
