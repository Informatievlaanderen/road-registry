namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using BackOffice.Abstractions.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegsegmentStatus : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegsegmentStatus(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task WegsegmentStatus_WegsegmentStatusVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.WegsegmentStatus.ToString(),
                Attribuutwaarde = null,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "WegsegmentStatusVerplicht", null);
    }

    [Fact]
    public async Task WegsegmentStatus_WegsegmentStatusNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.WegsegmentStatus.ToString(),
                Attribuutwaarde = string.Empty,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "WegsegmentStatusNietCorrect", null);
    }
}
