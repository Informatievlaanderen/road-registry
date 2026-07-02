namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.Organization;

using AutoFixture;
using FluentAssertions;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.AggregateTests.Framework;
using Xunit.Abstractions;

public class OrganizationMartenSerializerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OrganizationMartenSerializerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CanSerializeAndDeserializeToJson()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var original = fixture.Create<OrganizationReadItem>();

        var serializer = new StoreOptions().ConfigureSerializer().Serializer();
        var originalAsJson = serializer.ToJson(original);
        var deserialized = serializer.FromJson<OrganizationReadItem>(originalAsJson);

        _testOutputHelper.WriteLine($"Expected:\n{serializer.ToJson(original)}");
        _testOutputHelper.WriteLine($"\nActual:\n{serializer.ToJson(deserialized)}");

        deserialized.Should().BeEquivalentTo(original);
    }
}
