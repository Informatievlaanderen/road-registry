namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidToegangsbeperking : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidToegangsbeperking(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Toegangsbeperking_ToegangsbeperkingNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Toegangsbeperking = string.Empty,
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "ToegangsbeperkingNietCorrect", null);
    }
}
