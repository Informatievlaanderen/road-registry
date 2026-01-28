namespace RoadRegistry.Projections.Tests.Projections.ExtractGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Marten;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.AggregateTests.Framework;
using Xunit.Abstractions;

public class MartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = fixture.Create<GradeSeparatedJunctionExtractItem>();

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<GradeSeparatedJunctionExtractItem>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
