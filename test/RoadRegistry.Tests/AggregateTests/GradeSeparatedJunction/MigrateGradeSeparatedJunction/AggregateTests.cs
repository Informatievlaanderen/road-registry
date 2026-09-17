namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.MigrateGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
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
    public void WhenMigratedWithoutGeometryChange_ThenGeometryFollowsMigration()
    {
        // Arrange
        var geometry = new Point(5, 0).ToJunctionGeometry();
        var junction = CreateV1Junction(geometry);
        junction.Migrate(CreateMigrateChange(junction), TestData.Provenance);

        // Act
        junction.EnsureGeometryFollowsMigration(TestData.Provenance);

        // Assert
        var changes = junction.GetChanges().ToList();
        changes.Should().HaveCount(2);
        changes[0].Should().BeOfType<GradeSeparatedJunctionWasMigrated>();
        changes[1].Should().BeOfType<GradeSeparatedJunctionGeometryWasChanged>()
            .Which.Geometry.Should().Be(geometry);
    }

    [Fact]
    public void WhenMigratedAndGeometryChangedAfterwards_ThenNoExtraGeometryChange()
    {
        // Arrange
        var junction = CreateV1Junction(new Point(5, 0).ToJunctionGeometry());
        junction.Migrate(CreateMigrateChange(junction), TestData.Provenance);
        junction.ChangeGeometry(new Point(6, 0).ToJunctionGeometry(), TestData.Provenance);

        // Act
        junction.EnsureGeometryFollowsMigration(TestData.Provenance);

        // Assert
        junction.GetChanges().OfType<GradeSeparatedJunctionGeometryWasChanged>().Should().ContainSingle();
    }

    [Fact]
    public void WhenNotMigrated_ThenNoGeometryChange()
    {
        // Arrange
        var junction = CreateV1Junction(new Point(5, 0).ToJunctionGeometry());

        // Act
        junction.EnsureGeometryFollowsMigration(TestData.Provenance);

        // Assert
        junction.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenMigratedWithoutGeometry_ThenNoGeometryChange()
    {
        // Arrange
        var junction = CreateV1Junction(null);
        junction.Migrate(CreateMigrateChange(junction), TestData.Provenance);

        // Act
        junction.EnsureGeometryFollowsMigration(TestData.Provenance);

        // Assert
        junction.GetChanges().OfType<GradeSeparatedJunctionGeometryWasChanged>().Should().BeEmpty();
    }

    private GradeSeparatedJunction CreateV1Junction(JunctionGeometry? geometry)
    {
        return GradeSeparatedJunction.CreateForMigration(
            Fixture.Create<GradeSeparatedJunctionId>(),
            new RoadSegmentId(1),
            new RoadSegmentId(2),
            geometry);
    }

    private MigrateGradeSeparatedJunctionChange CreateMigrateChange(GradeSeparatedJunction junction)
    {
        return Fixture.Create<MigrateGradeSeparatedJunctionChange>() with
        {
            GradeSeparatedJunctionId = junction.GradeSeparatedJunctionId
        };
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
