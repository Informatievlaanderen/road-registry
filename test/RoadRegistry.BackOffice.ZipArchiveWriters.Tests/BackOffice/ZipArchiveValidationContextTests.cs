namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using System.Collections.Immutable;
using FluentAssertions;
using Uploads;

public class ZipArchiveValidationContextTests
{
    [Theory]
    [MemberData(nameof(WithRoadNodesTestCasesTestHelper))]
    public void WithRoadNodeReturnsExpectedResult(RecordType recordType, Func<ZipArchiveValidationContext, IImmutableSet<RoadNodeId>> getExpectedCollection)
    {
        // GIVEN
        var context = ZipArchiveValidationContext.Empty;
        var roadNodeId = new RoadNodeId(1);

        // WHEN
        var actualContext = context.WithRoadNode(roadNodeId, recordType);

        // THEN
        getExpectedCollection(actualContext).Should().Equal(roadNodeId);
        actualContext.KnownRoadNodes.Should().Equal(roadNodeId);
    }

    public static IEnumerable<(RecordType, Func<ZipArchiveValidationContext, IImmutableSet<RoadNodeId>>)> WithRoadNodesTestCases()
    {
        yield return (RecordType.Added, ctx => ctx.KnownAddedRoadNodes);
        yield return (RecordType.Identical, ctx => ctx.KnownIdenticalRoadNodes);
        yield return (RecordType.Modified, ctx => ctx.KnownModifiedRoadNodes);
        yield return (RecordType.Removed, ctx => ctx.KnownRemovedRoadNodes);
    }

    public static IEnumerable<object[]> WithRoadNodesTestCasesTestHelper()
    {
        return WithRoadNodesTestCases().Select(testCase => new object[] { testCase.Item1, testCase.Item2 });
    }
}
