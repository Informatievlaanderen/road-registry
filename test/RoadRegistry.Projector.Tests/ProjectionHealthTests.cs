namespace RoadRegistry.Projector.Tests;

using FluentAssertions;
using JasperFx;
using JasperFx.Events.Daemon;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Projector.Infrastructure;

// The status page used to read a projection's state straight off the daemon and only recognise Stopped, so a shard that
// had fallen over (Paused) rendered as healthy - which is how a stuck projection went unnoticed for thousands of
// events. These pin the distinction that fixes it: a problem is a disagreement between intent and reality, nothing else.
public class ProjectionHealthTests
{
    [Fact]
    public void APausedProjectionThatShouldBeRunningIsAProblem()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Subscribed, AgentStatus.Paused, 100)
            .Should().Contain("Paused").And.Contain("not processing events");
    }

    [Fact]
    public void AStoppedProjectionThatShouldBeRunningIsAProblem()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Subscribed, AgentStatus.Stopped, 100)
            .Should().Contain("not processing events");
    }

    // The whole point of persisting intent: this one is not an incident, and must not read as one.
    [Fact]
    public void AStoppedProjectionThatIsMeantToBeStoppedIsNotAProblem()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Stopped, AgentStatus.Stopped, 100)
            .Should().BeEmpty();
    }

    [Fact]
    public void APausedProjectionThatIsMeantToBeStoppedIsNotAProblem()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Stopped, AgentStatus.Paused, 100)
            .Should().BeEmpty();
    }

    [Fact]
    public void ARunningProjectionThatShouldBeRunningIsNotAProblem()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Subscribed, AgentStatus.Running, 100)
            .Should().BeEmpty();
    }

    [Fact]
    public void ARunningProjectionThatShouldBeStoppedIsAProblem()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Stopped, AgentStatus.Running, 100)
            .Should().Contain("supposed to be stopped");
    }

    // The case that motivated the whole change, in its quietest form: a projection somebody stopped, which has never
    // recorded a position. "No progression found" is what "has not run yet" looks like, and reporting it here would
    // make a deliberate stop read as an incident all over again.
    [Fact]
    public void AnIntentionallyStoppedProjectionWithNoProgressionIsQuiet()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Stopped, AgentStatus.Stopped, null)
            .Should().BeEmpty();
    }

    [Fact]
    public void AnIntentionallyStoppedProjectionThatIsStillRunningIsReportedEvenWithNoProgression()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Stopped, AgentStatus.Running, null)
            .Should().Contain("supposed to be stopped");
    }

    [Fact]
    public void AMissingProgressionIsStillReportedWhenNothingElseIsWrong()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Subscribed, AgentStatus.Running, null)
            .Should().Be("No progression found");
    }

    // A stuck projection matters more than a missing progression, so it must not be masked by it.
    [Fact]
    public void BeingStuckOutranksAMissingProgression()
    {
        ProjectionHealth.DescribeProblem(ProjectionDesiredStates.Subscribed, AgentStatus.Paused, null)
            .Should().Contain("Paused");
    }

    [Theory]
    [InlineData(AgentStatus.Running, "running")]
    [InlineData(AgentStatus.Paused, "paused")]
    [InlineData(AgentStatus.Stopped, "stopped")]
    public void TheActualStatusIsReportedAsItself(AgentStatus status, string expected)
    {
        ProjectionHealth.DescribeAgentStatus(status).Should().Be(expected);
    }

    [Fact]
    public void AnAbsentDaemonReportsUnknownRatherThanGuessing()
    {
        ProjectionHealth.DescribeAgentStatus(null).Should().Be("unknown");
    }
}
