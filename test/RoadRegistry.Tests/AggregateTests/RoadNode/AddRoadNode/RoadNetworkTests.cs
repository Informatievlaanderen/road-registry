namespace RoadRegistry.Tests.AggregateTests.RoadNode.AddRoadNode;

using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using ValueObjects.Problems;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .Then((result, events) => { result.Summary.RoadNodes.Added.Should().HaveCount(2); })
        );
    }

    [Fact]
    public Task WithInvalidChange_ThenProblems()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
            )
            .Then((result, events) => { result.Problems.Should().Contain(x => x.Reason.StartsWith("RoadNode")); })
        );
    }

    [Fact]
    public Task WhenAddingMultipleNodesWithSameTemporaryId_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1StartNode)
            )
            .ThenContainsProblems(new Error("RoadNodeTemporaryIdNotUnique",
                new ProblemParameter("TemporaryId", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                new ProblemParameter("WegknoopId", TestData.AddSegment1StartNode.TemporaryId.ToString())
            ))
        );
    }

    // An upload is built against an extract that may already be out of date: a road node another inwinning added on
    // a point afterwards is not in it, so the feature compare asks for a road node to be added there all the same.
    // The road node that is already there is used instead of a second one next to it (GAWR-7847).
    private static readonly Point Point1 = new Point(600000, 600000).WithSrid(WellknownSrids.Lambert08);
    private static readonly Point Point2 = new Point(600075, 600000).WithSrid(WellknownSrids.Lambert08);
    private static readonly Point Point3 = new Point(600100, 600000).WithSrid(WellknownSrids.Lambert08);

    [Fact]
    public Task WhenARoadNodeIsAlreadyThere_ThenTheRoadSegmentHangsOffTheExistingRoadNode()
    {
        return Run(scenario => scenario
            .Given(given => GivenRoadSegment1EndingAt(given, grensknoop: true))
            .When(changes => changes
                // The road node the uploader thinks is new, on the point where road node 2 already sits.
                .Add(TestData.AddSegment1EndNode with
                {
                    TemporaryId = new RoadNodeId(1000001259),
                    Geometry = RoadNodeGeometry.Create(Point2),
                    Grensknoop = true
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(Point3),
                    Grensknoop = false
                })
                .Add(BuildSegment2())
            )
            .Then((result, events) =>
            {
                result.Problems.Should().HaveNoError();

                // Only the road node at the far end of the new road segment is added; the one that is already there
                // is used as it is.
                events.OfType<RoadNodeWasAdded>().Should().ContainSingle()
                    .Which.Geometry.Should().Be(RoadNodeGeometry.Create(Point3));

                events.OfType<RoadSegmentWasAdded>().Should().ContainSingle()
                    .Which.StartNodeId.Should().Be(new RoadNodeId(2));
            })
        );
    }

    // The same through the inwinning, which is where this actually happens: the upload was prepared against an
    // extract from before the neighbouring inwinning added the road node.
    [Fact]
    public Task WhenTheInwinningAddsARoadNodeThatIsAlreadyThere_ThenTheExistingRoadNodeIsUsed()
    {
        return Run(scenario => scenario
            .Given(given => GivenRoadSegment1EndingAt(given, grensknoop: true))
            .WhenMigrate(changes => changes
                .Add(TestData.AddSegment1EndNode with
                {
                    TemporaryId = new RoadNodeId(1000001259),
                    Geometry = RoadNodeGeometry.Create(Point2),
                    Grensknoop = true
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(Point3),
                    Grensknoop = false
                })
                .Add(BuildSegment2())
            )
            .Then((result, events) =>
            {
                result.Problems.Should().HaveNoError();

                events.OfType<RoadNodeWasAdded>().Should().ContainSingle()
                    .Which.Geometry.Should().Be(RoadNodeGeometry.Create(Point3));
                events.OfType<RoadSegmentWasAdded>().Should().ContainSingle()
                    .Which.StartNodeId.Should().Be(new RoadNodeId(2));
            })
        );
    }

    // What the upload says about the road node is applied to the one that is already there.
    [Fact]
    public Task WhenTheRoadNodeThatIsAlreadyThereHasOtherAttributes_ThenItIsModified()
    {
        return Run(scenario => scenario
            .Given(given => GivenRoadSegment1EndingAt(given, grensknoop: false))
            .WhenMigrate(changes => changes
                .Add(TestData.AddSegment1EndNode with
                {
                    TemporaryId = new RoadNodeId(1000001259),
                    Geometry = RoadNodeGeometry.Create(Point2),
                    Grensknoop = true
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(Point3),
                    Grensknoop = false
                })
                .Add(BuildSegment2())
            )
            .Then((result, events) =>
            {
                result.Problems.Should().HaveNoError();

                var roadNodeWasModified = events.OfType<RoadNodeWasModified>().Should().ContainSingle().Subject;
                roadNodeWasModified.RoadNodeId.Should().Be(new RoadNodeId(2));
                roadNodeWasModified.Grensknoop.Should().BeTrue();

                result.Summary.RoadNodes.Modified.Should().Contain(new RoadNodeId(2));
                result.Summary.RoadNodes.Added.Should().NotContain(new RoadNodeId(2));

                // A grensknoop holds the two roads meeting there apart, so there is nothing to merge.
                events.OfType<RoadSegmentWasMerged>().Should().BeEmpty();
            })
        );
    }

    // The casus: the road node that is already there ends up carrying two road segments, and the inwinning may merge
    // the two roads meeting at it. Nothing about that merge is particular to this - it is the road node verification
    // doing what it does for any road node - but it is exactly what has to keep working here.
    [Fact]
    public Task WhenARoadSegmentIsAddedOnTheRoadNodeThatIsAlreadyThere_ThenTheRoadSegmentsAreMerged()
    {
        return Run(scenario => scenario
            .Given(given => GivenRoadSegment1EndingAt(given, grensknoop: false))
            .WhenMigrate(changes => changes
                .Add(TestData.AddSegment1EndNode with
                {
                    TemporaryId = new RoadNodeId(1000001259),
                    Geometry = RoadNodeGeometry.Create(Point2),
                    Grensknoop = false
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = RoadNodeGeometry.Create(Point3),
                    Grensknoop = false
                })
                .Add(BuildSegment2())
            )
            .Then((result, events) =>
            {
                result.Problems.Should().HaveNoError();

                // The road node that is already there was used, not a second one next to it.
                events.OfType<RoadNodeWasAdded>().Should().ContainSingle()
                    .Which.Geometry.Should().Be(RoadNodeGeometry.Create(Point3));

                // And it turns out not to be needed: the two roads meeting at it became one. Road segment 1 is three
                // quarters of what they became, so it keeps its identifier.
                events.OfType<RoadNodeWasRemoved>().Should().ContainSingle()
                    .Which.RoadNodeId.Should().Be(new RoadNodeId(2));

                var roadSegmentWasMerged = events.OfType<RoadSegmentWasMerged>().Should().ContainSingle().Subject;
                roadSegmentWasMerged.RoadSegmentId.Should().Be(new RoadSegmentId(1));
                roadSegmentWasMerged.OtherRoadSegmentId.Should().Be(new RoadSegmentId(2));

                events.OfType<RoadSegmentWasRetiredBecauseOfMerger>().Should().ContainSingle()
                    .Which.RoadSegmentId.Should().Be(new RoadSegmentId(2));
            })
        );
    }

    // Two road nodes the upload itself puts on one and the same point is a mistake in the upload, and stays reported
    // as such: there is no road node that was already there to fall back on.
    [Fact]
    public Task WhenTwoRoadNodesAreAddedOnOnePointInOneUpload_ThenGeometryTaken()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = TestData.AddSegment1StartNode.Geometry
                })
            )
            .ThenContainsProblems(new Error("RoadNodeGeometryTaken",
                new ProblemParameter("ByOtherNode", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                new ProblemParameter("WegknoopId", TestData.AddSegment1EndNode.TemporaryId.ToString())
            ))
        );
    }

    // Road node 1 on Point1, road node 2 on Point2 and road segment 1 running between them: the situation an earlier
    // inwinning left behind, and which the uploads above were not built against.
    private RoadNetworkChanges GivenRoadSegment1EndingAt(RoadNetworkChanges given, bool grensknoop)
    {
        return given
            .Add(TestData.AddSegment1StartNode with
            {
                Geometry = RoadNodeGeometry.Create(Point1),
                Grensknoop = false
            })
            .Add(TestData.AddSegment1EndNode with
            {
                Geometry = RoadNodeGeometry.Create(Point2),
                Grensknoop = grensknoop
            })
            .Add((TestData.AddSegment1 with
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(101)),
                Geometry = BuildRoadSegmentGeometry(Point1, Point2),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
                Status = RoadSegmentStatusV2.Gerealiseerd
            }).WithDynamicAttributePositionsOnEntireGeometryLength());
    }

    private AddRoadSegmentChange BuildSegment2()
    {
        return (TestData.AddSegment2 with
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(new RoadSegmentId(102)),
            Geometry = BuildRoadSegmentGeometry(Point2, Point3),
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            Status = RoadSegmentStatusV2.Gerealiseerd
        }).WithDynamicAttributePositionsOnEntireGeometryLength();
    }
}
