namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenChangingRoadSegmentsStreetNames;

using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadSegment.ValueObjects;

public class GivenRoadNetwork : RoadNetworkTestBase
{
    public GivenRoadNetwork(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task StateCheck()
    {
        var @event = new RoadSegmentsStreetNamesChanged
        {
            RoadSegments =
            [
                new RoadSegmentStreetNamesChanged
                {
                    Id = TestData.Segment1Added.Id,
                    Version = TestData.Segment1Added.Version,
                    GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
                    LeftSideStreetNameId = ObjectProvider.CreateWhichIsDifferentThan(TestData.Segment1Added.LeftSide.StreetNameId),
                    RightSideStreetNameId = ObjectProvider.CreateWhichIsDifferentThan(TestData.Segment1Added.RightSide.StreetNameId),
                }
            ]
        };
        await Given(RoadNetworks.Stream,
            new RoadNetworkChangesAcceptedBuilder(TestData)
                .WithRoadNodeAdded(TestData.StartNode1Added)
                .WithRoadNodeAdded(TestData.EndNode1Added)
                .WithRoadSegmentAdded(TestData.Segment1Added)
                .Build()
            , @event);

        // Assert
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);

        var actual = roadNetwork.FindRoadSegment(new RoadSegmentId(TestData.Segment1Added.Id));
        var expected = @event.RoadSegments.Single();
        actual.Version.ToInt32().Should().Be(expected.Version);
        actual.AttributeHash.LeftStreetNameId?.ToInt32().Should().Be(expected.LeftSideStreetNameId);
        actual.AttributeHash.RightStreetNameId?.ToInt32().Should().Be(expected.RightSideStreetNameId);
    }
}
