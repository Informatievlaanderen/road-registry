namespace RoadRegistry.Projector.Tests.Infrastructure;

using System.Collections.Generic;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Projector.Infrastructure;

public class MartenProjectionDesiredStatesTests
{
    private static readonly ProjectionDetail Projection = new()
    {
        Id = "RoadNetworkChangesReadProjection:All",
        Name = "V2 - Read",
        Description = "",
        FallbackDesiredState = ProjectionDesiredStates.Subscribed
    };

    [Fact]
    public void A_shard_without_a_row_falls_back_to_what_it_was_registered_with()
    {
        var desiredStates = new Dictionary<string, string>();

        Assert.Equal(ProjectionDesiredStates.Subscribed, desiredStates.DesiredStateOf(Projection));
    }

    [Fact]
    public void A_stored_state_wins_over_the_fallback()
    {
        var desiredStates = new Dictionary<string, string> { [Projection.Id] = ProjectionDesiredStates.Stopped };

        Assert.Equal(ProjectionDesiredStates.Stopped, desiredStates.DesiredStateOf(Projection));
        Assert.False(desiredStates.DesiredStateOf(Projection).ShouldBeRunning());
    }

    [Theory]
    [InlineData("subscribed", true)]
    [InlineData("SUBSCRIBED", true)]
    [InlineData("stopped", false)]
    [InlineData("Stopped", false)]
    // Anything unrecognised errs towards keeping the projection alive rather than leaving it silently down.
    [InlineData("something else", true)]
    public void Only_stopped_means_leave_it_alone(string desiredState, bool shouldBeRunning)
    {
        Assert.Equal(shouldBeRunning, desiredState.ShouldBeRunning());
    }
}
