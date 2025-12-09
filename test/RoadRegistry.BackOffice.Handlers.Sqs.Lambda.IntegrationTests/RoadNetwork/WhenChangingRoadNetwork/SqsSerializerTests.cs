namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using AutoFixture;
using CommandHandling.Actions.ChangeRoadNetwork;
using FluentAssertions;
using GradeSeparatedJunction.Changes;
using Newtonsoft.Json;
using RoadNode.Changes;
using RoadSegment.Changes;
using Tests.AggregateTests;
using Xunit.Abstractions;

public class SqsSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SqsSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var changes = fixture.CreateMany<ChangeRoadNetworkCommandItem>(1).ToList();

        // ensure null values are properly handled
        changes.Add(new ChangeRoadNetworkCommandItem
        {
            ModifyRoadNode = new ModifyRoadNodeChange
            {
                RoadNodeId = new RoadNodeId(1)
            },
            ModifyRoadSegment = new ModifyRoadSegmentChange
            {
                RoadSegmentId = new RoadSegmentId(1)
            },
            ModifyGradeSeparatedJunction = new ModifyGradeSeparatedJunctionChange
            {
                GradeSeparatedJunctionId = new  GradeSeparatedJunctionId(1)
            }
        });


        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var changesAsJson = JsonConvert.SerializeObject(changes, jsonSerializerSettings);
        var deserializedChanges = JsonConvert.DeserializeObject<ChangeRoadNetworkCommandItem[]>(changesAsJson, jsonSerializerSettings);

        _testOutputHelper.WriteLine($"Expected:\n{JsonConvert.SerializeObject(changes, Formatting.Indented, jsonSerializerSettings)}");
        _testOutputHelper.WriteLine($"\nActual:\n{JsonConvert.SerializeObject(deserializedChanges, Formatting.Indented, jsonSerializerSettings)}");

        deserializedChanges.Should().BeEquivalentTo(changes);
    }
}
