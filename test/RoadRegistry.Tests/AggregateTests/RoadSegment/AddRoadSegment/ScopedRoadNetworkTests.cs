namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.ValueObjects;
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
            .Then((result, events) =>
            {
                result.Summary.RoadSegments.Added.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WithInvalidChange_ThenProblems()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1 with
                {
                    Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                        .Add(RoadSegmentCategoryV2.EuropeseHoofdweg, TestData.AddSegment1.Geometry)
                        .Add(RoadSegmentCategoryV2.InterlokaleWeg, TestData.AddSegment1.Geometry)
                })
            )
            .Then((result, events) => { result.Problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment"); })
        );
    }

    [Fact]
    public Task WhenAddingMultipleSegmentsWithSameTemporaryId_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
                .Add(TestData.AddSegment2StartNode)
                .Add(TestData.AddSegment2EndNode)
                .Add(TestData.AddSegment2 with { RoadSegmentIdReference = TestData.AddSegment1.RoadSegmentIdReference })
            )
            .ThenContainsProblems(new Error("RoadSegmentTemporaryIdNotUnique",
                new ProblemParameter("WegsegmentId", TestData.AddSegment1.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentTempIds", TestData.AddSegment1.RoadSegmentIdReference.GetTempIdsAsString())
            ))
        );
    }
}
