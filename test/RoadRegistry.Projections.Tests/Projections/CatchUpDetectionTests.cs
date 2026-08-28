namespace RoadRegistry.Projections.Tests.Projections;

using FluentAssertions;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// The catch-up flag decides whether a projection takes its read model's indexes apart before replaying, so the boundary
// between "at the tail" and "behind" has to be exact. The high water mark is the store's ceiling: a first batch that
// reaches it has nothing after it.
public class CatchUpDetectionTests
{
    [Theory]
    [InlineData(1L, 1000L)]
    [InlineData(999L, 1000L)]
    public void ABatchBelowTheHighWaterMarkIsBehind(long batchMaxSequence, long highWaterMark)
    {
        RoadNetworkChangesProjection.IsBehind(batchMaxSequence, highWaterMark).Should().BeTrue();
    }

    // The case that matters for a projection that is already up to date: one new event arrives, the high water mark
    // moves to it, and the batch carrying it reaches exactly that. There is nothing to catch up on.
    [Fact]
    public void ABatchThatReachesTheHighWaterMarkIsNotBehind()
    {
        RoadNetworkChangesProjection.IsBehind(1000L, 1000L).Should().BeFalse();
    }

    [Fact]
    public void ABatchBeyondTheHighWaterMarkIsNotBehind()
    {
        RoadNetworkChangesProjection.IsBehind(1001L, 1000L).Should().BeFalse();
    }

    [Fact]
    public void AnEmptyStoreIsNotBehind()
    {
        RoadNetworkChangesProjection.IsBehind(0L, 0L).Should().BeFalse();
    }
}
