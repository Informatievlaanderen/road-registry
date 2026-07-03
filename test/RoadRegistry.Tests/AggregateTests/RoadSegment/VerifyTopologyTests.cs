namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class VerifyTopologyTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenStartNodeIsUnknown_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
            )
        );
    }

    [Fact]
    public Task WhenStartNodeIsRemoved_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(1)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
            )
        );
    }

    [Fact]
    public Task WhenStartNodeIsNotEqualToSegmentGeometryStartPoint_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = new Point(-10, 0).ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentStartNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
            )
        );
    }

    [Fact]
    public Task WhenEndNodeIsUnknown_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
            )
        );
    }

    [Fact]
    public Task WhenEndNodeIsRemoved_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(2)
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
            )
        );
    }

    [Fact]
    public Task WhenEndNodeIsNotEqualToSegmentGeometryEndPoint_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = new Point(-10, 0).ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(
                new Error("RoadSegmentEndNodeMissing",
                    new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                    new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
                )
            )
        );
    }

    [Fact]
    public Task WhenGeometryIntersectsWithOtherGeometryWithoutGradeSeparatedJunction_ThenError()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(10, -5);

        var segment1Geometry = BuildRoadSegmentGeometry(segment1Start, segment1End);
        var segment2Geometry = BuildRoadSegmentGeometry(segment2Start, segment2End);

        // Pin overlapping traffic directions so both segments are car-accessible at the intersection.
        // Otherwise the randomly generated directions sometimes don't overlap, in which case a grade
        // separated junction is added automatically instead of raising the expected error (flaky test).
        var carTrafficDirection1 = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>().Add(RoadSegmentTrafficDirection.Both, segment1Geometry);
        var carTrafficDirection2 = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>().Add(RoadSegmentTrafficDirection.Both, segment2Geometry);

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1Start.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1End.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = segment1Geometry,
                    CarTrafficDirection = carTrafficDirection1,
                    RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference with
                    {
                        TempIds = null
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(TestData.AddSegment2StartNode with
                {
                    Geometry = segment2Start.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = segment2End.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment2 with
                {
                    Geometry = segment2Geometry,
                    CarTrafficDirection = carTrafficDirection2,
                    RoadSegmentIdReference = TestData.AddSegment2.RoadSegmentIdReference with
                    {
                        TempIds = [new RoadSegmentTempId(3)]
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .ThenContainsProblems(new Error("IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                new ProblemParameter("IntersectingWegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentId", TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentTempIds", "3")
            ))
        );
    }

    [Fact]
    public Task WhenGeometryIntersectsWithOtherGeometryWithoutGradeSeparatedJunction_WithIntersectionAtStartOrEndNodes_ThenNoProblems()
    {
        var segment1 = (MultiLineString)new WKTReader().Read("MULTILINESTRING ((536627.52 702836.71, 536638.52 702839.92, 536639.31 702840.27, 536651.24 702845.69, 536660.66 702849.97))")
            .WithSrid(WellknownSrids.Lambert08);
        var segment2 = (MultiLineString)new WKTReader().Read("MULTILINESTRING ((536627.52 702836.71, 536627.50 702837.33, 536627.77 702844.09, 536628.33 702848.43, 536631.81 702883.88, 536641.54 702874.86, 536655.04 702857.45, 536660.66 702849.97))")
            .WithSrid(WellknownSrids.Lambert08);

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = segment1.ToRoadSegmentGeometry(),
                    RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference with
                    {
                        TempIds = null
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add((TestData.AddSegment2 with
                {
                    Geometry = segment2.ToRoadSegmentGeometry(),
                    RoadSegmentIdReference = TestData.AddSegment2.RoadSegmentIdReference with
                    {
                        TempIds = [new RoadSegmentTempId(3)]
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .Then((result, _) =>
            {
                result.Problems.Should().BeEmpty();
            })
        );
    }

    [Fact]
    public Task WhenGeometryIntersectsMultipleTimesWithOtherGeometry_ThenError()
    {
        var segment1Geometry = BuildRoadSegmentGeometry([new(0, 0), new(8, 0)]);
        var segment2Geometry = BuildRoadSegmentGeometry([new Point(2, 5), new(4, -5), new(6, 5)]);

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1Geometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1Geometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment1 with
                {
                    Geometry = segment1Geometry,
                    RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference with
                    {
                        TempIds = null
                    }
                }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .When(changes => changes
                .Add(TestData.AddSegment2StartNode with
                {
                    Geometry = segment2Geometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry()
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = segment2Geometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry()
                })
                .Add((TestData.AddSegment2 with
                {
                    Geometry = segment2Geometry,
                    RoadSegmentIdReference = new(new RoadSegmentId(2), [new RoadSegmentTempId(3)])
                }).WithDynamicAttributePositionsOnEntireGeometryLength()))
            .ThenContainsProblems(new Error("RoadSegmentDuplicateIntersections",
                new ProblemParameter("IntersectingWegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentId", "2"),
                new ProblemParameter("WegsegmentTempIds", "3")
            ))
        );
    }

    [Fact]
    public Task WhenOnlySegmentIsRemoved_ThenRoadNodeVerifyTopologyIsUsedToReturnError()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .When(changes => changes
                .Add(new RemoveRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId
                })
            )
            .ThenProblems(
                new Error("RoadNodeNotConnectedToAnySegment",
                    new ProblemParameter("WegknoopId", TestData.Segment1StartNodeAdded.RoadNodeId.ToString())
                ),
                new Error("RoadNodeNotConnectedToAnySegment",
                    new ProblemParameter("WegknoopId", TestData.Segment1EndNodeAdded.RoadNodeId.ToString())
                )
            )
        );
    }

    [Theory]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        false
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        true
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601004, 601005.01 601004, 601005.01 601006, 601010 601006))",
        false
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601004, 601005.01 601004, 601005.01 601006, 601010 601006))",
        true
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005 601000, 601000 601005, 601005 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        false
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005 601000, 601000 601005, 601005 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        true
    )]
    public Task GivenPartiallyOverlappingSegments_ThenError(string segment1Geometry, string segment2Geometry, bool swapGeometry)
    {
        // Arrange
        var existingGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(swapGeometry ? segment1Geometry : segment2Geometry).WithSrid(WellknownSrids.Lambert08));
        var newGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(swapGeometry ? segment2Geometry : segment1Geometry).WithSrid(WellknownSrids.Lambert08));

        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode with { Geometry = existingGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry() })
                .Add(TestData.AddSegment1EndNode with { Geometry = existingGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry() })
                .Add((TestData.AddSegment1 with { Geometry = existingGeometry }).WithDynamicAttributePositionsOnEntireGeometryLength()))
            .When(changes => changes
                .Add(TestData.AddSegment2StartNode with { Geometry = newGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry() })
                .Add(TestData.AddSegment2EndNode with { Geometry = newGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry() })
                .Add((TestData.AddSegment2 with { Geometry = newGeometry }).WithDynamicAttributePositionsOnEntireGeometryLength())
            )
            .ThenContainsProblems(new Error("RoadSegmentPartiallyOverlapsWithAnotherRoadSegment",
                new ProblemParameter("OtherWegsegmentId", TestData.Segment1Added.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentId", TestData.AddSegment2.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentTempIds", TestData.AddSegment2.RoadSegmentIdReference.GetTempIdsAsString())
            ))
        );
    }

    [Theory]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))"
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601005, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))"
    )]
    public async Task GivenNoPartiallyOverlappingSegments_ThenResult(string segment1Geometry, string segment2Geometry)
    {
        var reversedValues = new[] { false, true };
        foreach (var reversedValue in reversedValues)
        {
            var existingGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(reversedValue ? segment1Geometry : segment2Geometry).WithSrid(WellknownSrids.Lambert08));
            var newGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(reversedValue ? segment2Geometry : segment1Geometry).WithSrid(WellknownSrids.Lambert08));

            await Run(scenario => scenario
                .Given(given => given
                    .Add(TestData.AddSegment1StartNode with { Geometry = existingGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry() })
                    .Add(TestData.AddSegment1EndNode with { Geometry = existingGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry() })
                    .Add((TestData.AddSegment1 with { Geometry = existingGeometry }).WithDynamicAttributePositionsOnEntireGeometryLength()))
                .When(changes => changes
                    .Add(TestData.AddSegment2StartNode with { Geometry = newGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry() })
                    .Add(TestData.AddSegment2EndNode with { Geometry = newGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry() })
                    .Add((TestData.AddSegment2 with { Geometry = newGeometry }).WithDynamicAttributePositionsOnEntireGeometryLength())
                )
                .Then((result, _) =>
                {
                    result.Problems.Should().NotContain(x => x.Reason == "RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
                })
            );
        }
    }

    [Theory]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601050 601000))",
        "MULTILINESTRING ((601000 601000, 601000 601020, 601050 601020, 601050 601000))"
    )]
    public async Task GivenNoPartiallyOverlappingSegments_SpecialCaseIntersectionsAreEqualToStartEndVerticesOfBothSegments_ThenResult(string segment1Geometry, string segment2Geometry)
    {
        var reversedValues = new[] { false, true };
        foreach (var reversedValue in reversedValues)
        {
            var existingGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(reversedValue ? segment1Geometry : segment2Geometry).WithSrid(WellknownSrids.Lambert08));
            var newGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(reversedValue ? segment2Geometry : segment1Geometry).WithSrid(WellknownSrids.Lambert08));

            await Run(scenario => scenario
                .Given(given => given
                    .Add(TestData.AddSegment1StartNode with { Geometry = existingGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry() })
                    .Add(TestData.AddSegment1EndNode with { Geometry = existingGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry() })
                    .Add((TestData.AddSegment1 with { Geometry = existingGeometry }).WithDynamicAttributePositionsOnEntireGeometryLength()))
                .When(changes => changes
                    .Add(TestData.AddSegment2StartNode with { Geometry = newGeometry.Value.GetSingleLineString().StartPoint.ToRoadNodeGeometry() })
                    .Add(TestData.AddSegment2EndNode with { Geometry = newGeometry.Value.GetSingleLineString().EndPoint.ToRoadNodeGeometry() })
                    .Add((TestData.AddSegment2 with { Geometry = newGeometry }).WithDynamicAttributePositionsOnEntireGeometryLength())
                )
                .Then((result, _) =>
                {
                    result.Problems.Should().NotContain(x => x.Reason == "RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
                })
            );
        }
    }
}
