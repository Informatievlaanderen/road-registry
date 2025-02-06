namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;

public class WhenNoAttribuut : ChangeAttributesTestBase
{
    [Fact]
    public async Task ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        }, "JsonInvalid", null);
    }
}
