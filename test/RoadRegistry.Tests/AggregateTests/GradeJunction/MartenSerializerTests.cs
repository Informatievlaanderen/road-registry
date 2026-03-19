namespace RoadRegistry.Tests.AggregateTests.GradeJunction;

using AutoFixture;
using FluentAssertions;
using Marten;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Tests.AggregateTests.Framework;

public class MartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AggregateCanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = GradeJunction.Create(fixture.Create<GradeJunctionWasAdded>());

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<GradeJunction>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
