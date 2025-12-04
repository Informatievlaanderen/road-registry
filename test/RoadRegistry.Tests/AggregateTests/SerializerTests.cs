namespace RoadRegistry.Tests.AggregateTests;

using FluentAssertions;
using Framework;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;

public class SerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EventsCanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var martenEventType = typeof(IMartenEvent);
        var eventTypes = martenEventType.Assembly.GetTypes().Where(x => x.IsAssignableTo(martenEventType) && !x.IsAbstract).ToArray();
        eventTypes.Should().NotBeEmpty();

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();

        foreach (var eventType in eventTypes)
        {
            _testOutputHelper.WriteLine(eventType.Name);

            var evt = fixture.Create(eventType);

            var json = serializer.ToJson(evt);
            var deserializedEvent = serializer.FromJson(eventType, json);

            _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(evt)}");
            _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserializedEvent)}");

            deserializedEvent.Should().BeEquivalentTo(evt);
        }
    }
}
