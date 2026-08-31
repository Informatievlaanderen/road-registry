namespace RoadRegistry.Projector.Infrastructure;

using RoadRegistry.Infrastructure.MartenDb.Projections;
using System;
using JasperFx;
using JasperFx.Events.Daemon;

// Reconciles what a projection is meant to be doing with what its shard is actually doing.
//
// The daemon has three states and they are not interchangeable: a shard that fell over is Paused, one that was told to
// stop is Stopped. Reading either as "not running" loses the only distinction that matters to an operator - whether
// somebody meant this.
public static class ProjectionHealth
{
    public static string DescribeAgentStatus(AgentStatus? status)
    {
        return status switch
        {
            null => "unknown",
            AgentStatus.Running => "running",
            AgentStatus.Paused => "paused",
            AgentStatus.Stopped => "stopped",
            _ => status.Value.ToString().ToLowerInvariant()
        };
    }

    // Only reports a problem when what is happening differs from what was asked for. A projection stopped because
    // somebody stopped it is not a problem; one paused or stopped while it is supposed to be running is stuck, and
    // without this the status page has nothing that says so.
    public static string DescribeProblem(string desiredState, AgentStatus? actualStatus, long? lastSequenceId)
    {
        if (IsDesired(desiredState, ProjectionDesiredStates.Stopped))
        {
            // A projection that is meant to be stopped has nothing to say unless it is somehow still running. A missing
            // progression in particular is what "has not run yet" looks like, not an incident - reporting it would make
            // a deliberate stop read as a failure, which is the confusion this whole change exists to remove.
            return actualStatus == AgentStatus.Running
                ? "Running while it is supposed to be stopped."
                : string.Empty;
        }

        // Anything that is not explicitly "stopped" is treated as supposed to be running, so an unrecognised desired
        // state errs towards reporting a problem rather than hiding one.
        switch (actualStatus)
        {
            case AgentStatus.Paused:
                return "Paused after an error while it is supposed to be running; it is not processing events. Check the logs for the failure.";
            case AgentStatus.Stopped:
                return "Stopped while it is supposed to be running; it is not processing events.";
        }

        return lastSequenceId is null ? "No progression found" : string.Empty;
    }

    private static bool IsDesired(string desiredState, string expected)
    {
        return string.Equals(desiredState, expected, StringComparison.OrdinalIgnoreCase);
    }
}
