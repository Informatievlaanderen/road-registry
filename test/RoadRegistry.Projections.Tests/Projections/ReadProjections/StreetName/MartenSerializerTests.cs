namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.StreetName;

using AutoFixture;
using FluentAssertions;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.AggregateTests.Framework;
using Xunit.Abstractions;

public class StreetNameMartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StreetNameMartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = fixture.Create<StreetNameReadItem>();

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<StreetNameReadItem>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
