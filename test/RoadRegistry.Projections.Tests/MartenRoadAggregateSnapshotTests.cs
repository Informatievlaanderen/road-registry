namespace RoadRegistry.Projections.Tests;

using FluentAssertions;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;

public class MartenRoadAggregateSnapshotTests
{
    [Fact]
    public void AddRoadAggregatesSnapshots_DoesNotThrowProjectionValidationError()
    {
        // Snapshot<T> assembles a single-stream aggregation and validates the aggregate's Create/Apply methods,
        // throwing if any static Create method has a parameter Marten cannot resolve (e.g. IEventOrdinalProvider).
        // The domain's ordinal-aware creator is therefore named CreateWithProvider so Marten does not discover it.
        var options = new StoreOptions();

        var act = () => options.AddRoadAggregatesSnapshots();

        act.Should().NotThrow();
    }
}
