namespace RoadRegistry.Tests.AggregateTests;

using FluentAssertions;
using Framework;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;

public class MartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EventsCanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var martenEventType = typeof(IMartenEvent);
        var eventTypes = martenEventType.Assembly.GetTypes().Where(x => x.IsAssignableTo(martenEventType) && !x.IsAbstract).ToArray();
        eventTypes.Should().NotBeEmpty();

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();

        foreach (var eventType in eventTypes)
        {
            _testOutputHelper.WriteLine(eventType.Name);

            var original = fixture.Create(eventType);

            var originalAsJson = serializer.ToJson(original);
            var deserialized = serializer.FromJson(eventType, originalAsJson);

            _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
            _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

            deserialized.Should().BeEquivalentTo(original);
        }
    }
}
