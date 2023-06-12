namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegsegmentStatus : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegsegmentStatus(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task WegsegmentStatus_WegsegmentStatusNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                WegsegmentStatus = string.Empty,
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "WegsegmentStatusNietCorrect", null);
    }
}
