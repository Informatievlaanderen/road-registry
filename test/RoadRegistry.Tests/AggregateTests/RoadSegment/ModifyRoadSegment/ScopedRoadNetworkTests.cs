namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .When(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten
                        ? RoadSegmentGeometryDrawMethodV2.Ingeschetst
                        : RoadSegmentGeometryDrawMethodV2.Ingemeten
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadSegments.Modified.Should().HaveCount(1);
            })
        );
    }

    // A road segment that is no longer realized comes loose from its road nodes; those are verified afterwards like any
    // other road node the change touched, so one left hanging off nothing has to go in the same upload.
    [Fact]
    public Task WhenNoLongerGerealiseerd_ThenTheRoadNodesItLeavesBehindAreVerified()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1 with { Status = RoadSegmentStatusV2.Gerealiseerd }))
            .When(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    Status = RoadSegmentStatusV2.Gepland
                })
            )
            .Then((result, _) =>
            {
                result.Problems.Where(x => x.Reason == "RoadNodeNotConnectedToAnySegment").Should().HaveCount(2);
            })
        );
    }

    [Fact]
    public Task WhenNoLongerGerealiseerd_AndTheRoadNodesItLeavesBehindAreRemoved_ThenNoError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1 with { Status = RoadSegmentStatusV2.Gerealiseerd }))
            .When(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    Status = RoadSegmentStatusV2.Gepland
                })
                .Add(new RemoveRoadNodeChange { RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId })
                .Add(new RemoveRoadNodeChange { RoadNodeId = TestData.Segment1EndNodeAdded.RoadNodeId })
            )
            .Then((result, _) =>
            {
                result.Problems.Should().HaveNoError();
                result.Summary.RoadSegments.Modified.Should().ContainSingle();
            })
        );
    }

    [Fact]
    public Task WhenNoLongerGerealiseerd_AndAnotherRoadSegmentStillHangsOffARoadNodeItLeavesBehind_ThenThatRoadNodeStays()
    {
        // Segment 2 starts where segment 1 ends, so that road node keeps a road segment after segment 1 comes loose.
        var sharedNode = TestData.EndPoint1;

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1 with { Status = RoadSegmentStatusV2.Gerealiseerd })
                .Add(TestData.AddSegment2EndNode)
                .Add((TestData.AddSegment2 with
                {
                    Geometry = BuildRoadSegmentGeometry(sharedNode, TestData.EndPoint2),
                    Status = RoadSegmentStatusV2.Gerealiseerd
                }).WithDynamicAttributePositionsOnEntireGeometryLength()))
            .When(changes => changes
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                    Status = RoadSegmentStatusV2.Gepland
                })
                .Add(new RemoveRoadNodeChange { RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId })
            )
            .Then((result, events) =>
            {
                result.Problems.Should().HaveNoError();
                events.OfType<RoadNodeWasRemoved>().Should().ContainSingle()
                    .Which.RoadNodeId.Should().Be(TestData.Segment1StartNodeAdded.RoadNodeId);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<ModifyRoadSegmentChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadSegmentNotFound",
                new ProblemParameter("WegsegmentId", change.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentTempIds", change.RoadSegmentIdReference.GetTempIdsAsString())
            ))
        );
    }
}
