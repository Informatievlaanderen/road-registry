namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction.Changes;
using Newtonsoft.Json;
using RoadNode.Changes;
using RoadSegment.Changes;
using Sqs.RoadNetwork;
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

        var original = fixture.Create<ChangeRoadNetworkSqsRequest>();
        var changes = fixture.CreateMany<ChangeRoadNetworkItem>(1).ToList();
        // ensure null values are properly handled
        changes.Add(new ChangeRoadNetworkItem
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
        original.Changes = changes;

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var originalAsJson = JsonConvert.SerializeObject(original, jsonSerializerSettings);
        var deserialized = JsonConvert.DeserializeObject<ChangeRoadNetworkSqsRequest>(originalAsJson, jsonSerializerSettings);

        _testOutputHelper.WriteLine($"Expected:\n{JsonConvert.SerializeObject(original, Formatting.Indented, jsonSerializerSettings)}");
        _testOutputHelper.WriteLine($"\nActual:\n{JsonConvert.SerializeObject(deserialized, Formatting.Indented, jsonSerializerSettings)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
