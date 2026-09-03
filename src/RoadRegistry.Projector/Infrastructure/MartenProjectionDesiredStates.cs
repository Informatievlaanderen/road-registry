namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// Reading the desired state of a Marten projection: the same two questions asked by everything that acts on it - the
// daemon deciding which shards to start, the supervisor deciding which to bring back, and the status page.
public static class MartenProjectionDesiredStates
{
    // No row means nobody ever expressed an intent, which leaves the projection with the state it was registered with.
    public static string DesiredStateOf(this IReadOnlyDictionary<string, string> desiredStates, ProjectionDetail projection)
    {
        return desiredStates.TryGetValue(projection.Id, out var desiredState)
            ? desiredState
            : projection.FallbackDesiredState;
    }

    // Anything that is not explicitly "stopped" counts as supposed to be running, so an unrecognised value errs
    // towards keeping the projection alive rather than silently leaving it down.
    public static bool ShouldBeRunning(this string desiredState)
    {
        return !string.Equals(desiredState, ProjectionDesiredStates.Stopped, StringComparison.OrdinalIgnoreCase);
    }
}
