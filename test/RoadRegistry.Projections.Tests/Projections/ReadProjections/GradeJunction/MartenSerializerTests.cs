namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.GradeJunction;

using AutoFixture;
using FluentAssertions;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.AggregateTests.Framework;
using Xunit.Abstractions;

public class GradeJunctionMartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GradeJunctionMartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = fixture.Create<GradeJunctionReadItem>();

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<GradeJunctionReadItem>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
