namespace RoadRegistry.Tests.BackOffice;

using FluentAssertions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.Messages;
using ValueObjects.ProblemCodes;
using ProblemSeverity = RoadRegistry.Infrastructure.Messages.ProblemSeverity;

public class ProblemTicketErrorTests
{
    [Fact]
    public void ContextParametersOfARejectedChangeEndUpInTheErrorContext()
    {
        // A rejected change only keeps a problem's context as ordinary parameters. Without it in the error context, a
        // bulk removal that fails gives no way to tell which of the removed road segments caused it.
        var problem = new Problem
        {
            Severity = ProblemSeverity.Error,
            Reason = ProblemCode.RoadNetwork.Disconnected.ToString(),
            Parameters =
            [
                new ProblemParameter { Name = "StartNodeId", Value = "167684" },
                new ProblemParameter { Name = "EndNodeId", Value = "208843" },
                new ProblemParameter { Name = "WegsegmentId", Value = "83842" }
            ]
        };

        var error = problem.ToTicketError(WellKnownProblemTranslators.Default);

        error.ErrorMessage.Should().Be("De wegknoop 167684 heeft geen verbinding meer met wegknoop 208843.");
        error.ErrorCode.Should().Be("ErrorWegenNetwerkNietVerbonden");
        error.ErrorContext.Should().BeEquivalentTo(new Dictionary<string, object> { { "WegsegmentId", 83842 } });
    }

    [Fact]
    public void ProblemWithoutContextParametersHasNoErrorContext()
    {
        var problem = new Problem
        {
            Severity = ProblemSeverity.Error,
            Reason = ProblemCode.RoadNetwork.Disconnected.ToString(),
            Parameters =
            [
                new ProblemParameter { Name = "StartNodeId", Value = "1" },
                new ProblemParameter { Name = "EndNodeId", Value = "2" }
            ]
        };

        var error = problem.ToTicketError(WellKnownProblemTranslators.Default);

        error.ErrorContext.Should().BeNull();
    }
}
