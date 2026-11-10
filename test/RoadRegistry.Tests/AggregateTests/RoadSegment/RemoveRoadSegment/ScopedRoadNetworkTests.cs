namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

// An upload never removes a road segment it leaves out: it historizes it, along the status path that leads there.
public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenRealized_ThenItIsHistorizedAndTheSummarySaysModified()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1 with { Status = RoadSegmentStatusV2.Gerealiseerd }))
            .When(changes => changes
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
                .Add(new RemoveRoadNodeChange { RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId })
                .Add(new RemoveRoadNodeChange { RoadNodeId = TestData.Segment1EndNodeAdded.RoadNodeId })
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
                result.Summary.RoadSegments.Removed.Should().BeEmpty();
                result.Summary.RoadSegments.Modified.Should().Contain(TestData.Segment1Added.RoadSegmentId);

                events.OfType<RoadSegmentWasRemoved>().Should().BeEmpty();
                events.OfType<RoadSegmentWasHistorizedFromRealized>().Should().ContainSingle()
                    .Which.RoadSegmentId.Should().Be(TestData.Segment1Added.RoadSegmentId);
            })
        );
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland), typeof(RoadSegmentWasHistorizedFromPlanned))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd), typeof(RoadSegmentWasHistorizedFromNotRealized))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik), typeof(RoadSegmentWasHistorizedFromOutOfUse))]
    public Task WhenNotRealized_ThenItIsHistorizedDirectly(string status, Type expectedEvent)
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1 with { Status = RoadSegmentStatusV2.Parse(status) }))
            .When(changes => changes
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
                events.OfType<RoadSegmentWasRemoved>().Should().BeEmpty();
                events.OfType<IRoadSegmentStatusChangeEvent>().Should().ContainSingle()
                    .Which.Should().BeOfType(expectedEvent);
            })
        );
    }

    [Fact]
    public Task WhenAlreadyHistorized_ThenNothingIsRecorded()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1 with { Status = RoadSegmentStatusV2.Gehistoreerd }))
            .When(changes => changes
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
                result.Summary.RoadSegments.Modified.Should().BeEmpty();
                events.OfType<IRoadSegmentStatusChangeEvent>().Should().BeEmpty();
                events.OfType<RoadSegmentWasRemoved>().Should().BeEmpty();
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenNoProblem()
    {
        var change = Fixture.Create<RemoveRoadSegmentChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
            })
        );
    }
}
