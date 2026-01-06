namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class RoadNetworkTests : RoadNetworkTestBase
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
            .Then((result, events) => { result.Summary.RoadSegments.Added.Should().HaveCount(1); })
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
                    Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                        .Add(Fixture.Create<RoadSegmentCategory>())
                        .Add(Fixture.Create<RoadSegmentCategory>())
                })
            )
            .Then((result, events) =>
            {
                result.Problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment");
            })
        );
    }

    [Fact]
    public Task WhenAddingMultipleSegmentsWithSameTemporaryId_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1)
                .Add(TestData.AddSegment1)
            )
            .ThenContainsProblems(new Error("RoadSegmentTemporaryIdNotUnique",
                new ProblemParameter("TemporaryId", TestData.AddSegment1.TemporaryId.ToString())
            ))
        );
    }
}
