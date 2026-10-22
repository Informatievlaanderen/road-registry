namespace RoadRegistry.Tests.BackOffice;

using FluentAssertions;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.Messages;
using ValueObjects.ProblemCodes;
using ProblemSeverity = RoadRegistry.Infrastructure.Messages.ProblemSeverity;

// The feature compare matches road nodes on geometry, so the road node a problem is about can carry an identifier the
// uploader has never seen. Where the upload's own WK_OIDN is known it travels with the problem, and both are reported.
public class OriginalRoadNodeIdTranslationTests
{
    public static TheoryData<string> RoadNodeProblemCodes =>
    [
        ProblemCode.RoadNode.NotConnectedToAnySegment.ToString(),
        ProblemCode.RoadNode.NotFound.ToString(),
        ProblemCode.RoadNode.TooClose.ToString(),
        ProblemCode.RoadNode.NotCompletedInwinning.ToString(),
        ProblemCode.RoadNode.IsNotAllowed.ToString()
    ];

    [Theory]
    [MemberData(nameof(RoadNodeProblemCodes))]
    public void WhenTheUploadKnowsItByAnotherIdentifier_ThenBothAreReported(string problemCode)
    {
        var message = Translate(WellKnownProblemTranslators.Extract, problemCode,
            new ProblemParameter("WegknoopId", "1052854"),
            new ProblemParameter("WegknoopOorspronkelijkeId", "123"),
            // Only read by 'de wegknoop ligt te dicht bij een wegsegment', which names the road node only when it
            // knows which road segment it is too close to.
            new ProblemParameter("OtherSegmentId", "999"));

        message.Should().Contain("WK_OIDN 1052854 (origineel 123)");
    }

    [Theory]
    [MemberData(nameof(RoadNodeProblemCodes))]
    public void WhenTheUploadKnowsItByTheSameIdentifier_ThenOnlyOneIsReported(string problemCode)
    {
        var message = Translate(WellKnownProblemTranslators.Extract, problemCode,
            new ProblemParameter("WegknoopId", "1052854"),
            new ProblemParameter("OtherSegmentId", "999"));

        message.Should().Contain("WK_OIDN 1052854");
        message.Should().NotContain("origineel");
    }

    [Fact]
    public void WhenTheProblemDidNotComeFromAnUpload_ThenOnlyTheRegistersIdentifierIsReported()
    {
        var message = Translate(WellKnownProblemTranslators.Default,
            ProblemCode.RoadNode.NotConnectedToAnySegment.ToString(),
            new ProblemParameter("WegknoopId", "1052854"));

        message.Should().Contain("id 1052854");
        message.Should().NotContain("origineel");
    }

    [Fact]
    public void WhenTheProblemCameFromAnApiRequestAboutAnUploadedRoadNode_ThenBothAreReported()
    {
        var message = Translate(WellKnownProblemTranslators.Default,
            ProblemCode.RoadNode.NotConnectedToAnySegment.ToString(),
            new ProblemParameter("WegknoopId", "1052854"),
            new ProblemParameter("WegknoopOorspronkelijkeId", "123"));

        message.Should().Contain("id 1052854 (origineel 123)");
    }

    private static string Translate(IProblemTranslator translator, string problemCode, params ProblemParameter[] parameters)
    {
        return translator.Translate(new Problem
        {
            Severity = ProblemSeverity.Error,
            Reason = problemCode,
            Parameters = parameters
        }).Message;
    }
}
