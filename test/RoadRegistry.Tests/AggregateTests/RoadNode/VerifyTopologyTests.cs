namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using Extensions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNode.Changes;
using ValueObjects.Problems;

public class VerifyTopologyTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenRemovedAndSegmentsAreStillConnectedByStartNode_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId
                })
            )
            .ThenProblems(new Error("RoadSegmentStartNodeMissing", new ProblemParameter("Identifier", "1")))
        );
    }

    [Fact]
    public Task WhenRemovedAndSegmentsAreStillConnectedByEndNode_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1EndNodeAdded.RoadNodeId
                })
            )
            .ThenProblems(new Error("RoadSegmentEndNodeMissing", new ProblemParameter("Identifier", "1")))
        );
    }

    [Fact]
    public Task WhenGeometryIsReasonablyEqualToOtherNodeGeometry_ThenError()
    {
        return Run(scenario => scenario
            .Given(g => g)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = new Point(TestData.AddSegment1StartNode.Geometry.Value.X + 0.0001, TestData.AddSegment1StartNode.Geometry.Value.Y).ToRoadNodeGeometry()
                })
            )
            .ThenContainsProblems(new Error("RoadNodeGeometryTaken", new ProblemParameter("ByOtherNode", TestData.AddSegment1StartNode.TemporaryId.ToString())))
        );
    }

    [Fact]
    public Task WhenNodeIsTooCloseToUnconnectedSegment_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .When(changes => changes
                .Add(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(3),
                    OriginalId = Fixture.Create<RoadNodeId>(),
                    Geometry = new Point(TestData.AddSegment1StartNode.Geometry.Value.X + 1.99, TestData.AddSegment1StartNode.Geometry.Value.Y).ToRoadNodeGeometry(),
                    Type = Fixture.Create<RoadNodeTypeV2>(),
                    Grensknoop = Fixture.Create<bool>()
                })
            )
            .ThenContainsProblems(new Error("RoadNodeTooClose", new ProblemParameter("ToOtherSegment", TestData.Segment1Added.RoadSegmentId.ToString())))
        );
    }

    [Fact]
    public Task WhenVerifyType_WithNoSegmentsConnected_ThenError()
    {
        return Run(scenario => scenario
            .Given(b => b)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
            )
            .ThenProblems(new Error("RoadNodeNotConnectedToAnySegment", new ProblemParameter("RoadNodeId", TestData.AddSegment1StartNode.TemporaryId.ToString())))
        );
    }

    [Theory]
    [InlineData(nameof(RoadNodeTypeV2.Eindknoop), false)]
    [InlineData(nameof(RoadNodeTypeV2.Schijnknoop), true)]
    [InlineData(nameof(RoadNodeTypeV2.EchteKnoop), true)]
    //[InlineData(nameof(RoadNodeTypeV2.Validatieknoop), true)] //TODO-pr implement
    public Task WhenVerifyType_WithOneSegmentConnectedAndTypeIsNotEndNode_ThenError(string roadNodeType, bool expectError)
    {
        var addStartNode1WithWrongType = TestData.AddSegment1StartNode with
        {
            Type = RoadNodeTypeV2.Parse(roadNodeType)
        };

        return Run(scenario =>
            {
                var when = scenario
                    .Given(b => b)
                    .When(changes => changes
                        .Add(addStartNode1WithWrongType)
                        .Add(TestData.AddSegment1)
                    );

                if (expectError)
                {
                    return when.ThenContainsProblems(new Error("RoadNodeTypeMismatch",
                        new ProblemParameter("RoadNodeId", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                        new ProblemParameter("ConnectedSegmentCount", "1"),
                        new ProblemParameter("ConnectedSegmentId", TestData.AddSegment1.TemporaryId.ToString()),
                        new ProblemParameter("Actual", addStartNode1WithWrongType.Type.ToString()),
                        new ProblemParameter("Expected", RoadNodeTypeV2.Eindknoop.ToString())
                    ));
                }

                return when.ThenProblems(problems => problems.All(x => x.Reason != "RoadNodeTypeMismatch"));
            }
        );
    }

    [Theory]
    [InlineData(nameof(RoadNodeTypeV2.Eindknoop), true)]
    [InlineData(nameof(RoadNodeTypeV2.Schijnknoop), false)]
    [InlineData(nameof(RoadNodeTypeV2.EchteKnoop), true)]
    //[InlineData(nameof(RoadNodeTypeV2.Validatieknoop), false)] //TODO-pr implement
    public Task WhenVerifyType_WithTwoSegmentsConnectedAndTypeIsNotFakeNodeOrTurningLoopNode_ThenError(string roadNodeType, bool expectError)
    {
        var addStartNode1WithWrongType = TestData.AddSegment1StartNode with
        {
            Type = RoadNodeTypeV2.Parse(roadNodeType)
        };

        return Run(scenario =>
            {
                var when = scenario
                    .Given(b => b)
                    .When(changes => changes
                        .Add(addStartNode1WithWrongType)
                        .Add(TestData.AddSegment1)
                        .Add(TestData.AddSegment2 with
                        {
                            StartNodeId = addStartNode1WithWrongType.TemporaryId
                        })
                    );

                if (expectError)
                {
                    return when.ThenContainsProblems(new Error("RoadNodeTypeMismatch",
                        new ProblemParameter("RoadNodeId", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                        new ProblemParameter("ConnectedSegmentCount", "2"),
                        new ProblemParameter("ConnectedSegmentId", TestData.AddSegment1.TemporaryId.ToString()),
                        new ProblemParameter("ConnectedSegmentId", TestData.AddSegment2.TemporaryId.ToString()),
                        new ProblemParameter("Actual", addStartNode1WithWrongType.Type.ToString()),
                        new ProblemParameter("Expected", RoadNodeTypeV2.Schijnknoop.ToString())
                    ));
                }

                return when.ThenProblems(problems => problems.All(x => x.Reason != "RoadNodeTypeMismatch"));
            }
        );
    }

    [Fact]
    public Task WhenVerifyType_WithTwoSegmentsConnectedAndTypeIsFakeNodeButConnectedSegmentAttributesAreIdentical_ThenError()
    {
        var addStartNode1WithWrongType = TestData.AddSegment1StartNode with
        {
            Type = RoadNodeTypeV2.Schijnknoop
        };

        var addIdenticalSegment = TestData.AddSegment1 with
        {
            TemporaryId = Fixture.CreateWhichIsDifferentThan(TestData.AddSegment1.TemporaryId),
            OriginalId = Fixture.CreateWhichIsDifferentThan(TestData.AddSegment1.TemporaryId, TestData.AddSegment1.OriginalId)
        };

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(addStartNode1WithWrongType)
                .Add(TestData.AddSegment1)
                .Add(addIdenticalSegment)
            )
            .ThenContainsProblems(new Error("FakeRoadNodeConnectedSegmentsDoNotDiffer",
                new ProblemParameter("RoadNodeId", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                new ProblemParameter("SegmentId", TestData.AddSegment1.OriginalId.ToString()),
                new ProblemParameter("SegmentId", addIdenticalSegment.OriginalId.ToString())
            ))
        );
    }

    [Theory]
    [InlineData(nameof(RoadNodeTypeV2.Eindknoop), true)]
    [InlineData(nameof(RoadNodeTypeV2.Schijnknoop), true)]
    [InlineData(nameof(RoadNodeTypeV2.EchteKnoop), false)]
    //[InlineData(nameof(RoadNodeTypeV2.Validatieknoop), true)] //TODO-pr implement
    public Task WhenVerifyType_WithThreeOrMoreSegmentsConnectedAndTypeIsNotRealNodeOrMiniRoundabout_ThenError(string roadNodeType, bool expectError)
    {
        var addStartNode1WithWrongType = TestData.AddSegment1StartNode with
        {
            Type = RoadNodeTypeV2.Parse(roadNodeType)
        };

        return Run(scenario =>
            {
                var when = scenario
                    .Given(b => b)
                    .When(changes => changes
                        .Add(addStartNode1WithWrongType)
                        .Add(TestData.AddSegment1)
                        .Add(TestData.AddSegment2 with
                        {
                            StartNodeId = addStartNode1WithWrongType.TemporaryId
                        })
                        .Add(TestData.AddSegment3 with
                        {
                            StartNodeId = addStartNode1WithWrongType.TemporaryId
                        })
                    );

                if (expectError)
                {
                    return when.ThenContainsProblems(new Error("RoadNodeTypeMismatch",
                        new ProblemParameter("RoadNodeId", TestData.AddSegment1StartNode.TemporaryId.ToString()),
                        new ProblemParameter("ConnectedSegmentCount", "3"),
                        new ProblemParameter("ConnectedSegmentId", TestData.AddSegment1.TemporaryId.ToString()),
                        new ProblemParameter("ConnectedSegmentId", TestData.AddSegment2.TemporaryId.ToString()),
                        new ProblemParameter("ConnectedSegmentId", TestData.AddSegment3.TemporaryId.ToString()),
                        new ProblemParameter("Actual", addStartNode1WithWrongType.Type.ToString()),
                        new ProblemParameter("Expected", RoadNodeTypeV2.EchteKnoop.ToString())
                    ));
                }

                return when.ThenProblems(problems => problems.All(x => x.Reason != "RoadNodeTypeMismatch"));
            }
        );
    }
}
