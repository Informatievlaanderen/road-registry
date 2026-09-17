namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.MigrateGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionMigrated()
    {
        // Arrange
        var junction = GradeSeparatedJunction.CreateForMigration(
            Fixture.Create<GradeSeparatedJunctionId>(),
            new RoadSegmentId(1),
            new RoadSegmentId(2),
            null);
        var change = Fixture.Create<MigrateGradeSeparatedJunctionChange>() with
        {
            GradeSeparatedJunctionId = junction.GradeSeparatedJunctionId
        };

        // Act
        var problems = junction.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        junction.GetChanges().Should().HaveCount(1);

        var junctionMigrated = (GradeSeparatedJunctionWasMigrated)junction.GetChanges().Single();
        junctionMigrated.GradeSeparatedJunctionId.Should().Be(junction.GradeSeparatedJunctionId);
        junctionMigrated.LowerRoadSegmentId.Should().Be(change.LowerRoadSegmentId);
        junctionMigrated.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
        junctionMigrated.Type.Should().Be(change.Type);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var junction = GradeSeparatedJunction.CreateForMigration(
            Fixture.Create<GradeSeparatedJunctionId>(),
            new RoadSegmentId(1),
            new RoadSegmentId(2),
            null);
        junction.HasMigrated().Should().BeFalse();

        var evt = Fixture.Create<GradeSeparatedJunctionWasMigrated>() with
        {
            GradeSeparatedJunctionId = junction.GradeSeparatedJunctionId
        };

        // Act
        junction.Apply(evt);

        // Assert
        junction.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        junction.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId);
        junction.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId);
        junction.Type.Should().Be(evt.Type);
        junction.IsRemoved.Should().BeFalse();
        junction.HasMigrated().Should().BeTrue();
    }
}
