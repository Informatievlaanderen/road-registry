namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.Parameters;
using BackOffice.Abstractions.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidToegangsbeperking : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidToegangsbeperking(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Toegangsbeperking_ToegangsbeperkingVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Toegangsbeperking.ToString(),
                Attribuutwaarde = null,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "ToegangsbeperkingVerplicht", null);
    }

    [Fact]
    public async Task Toegangsbeperking_ToegangsbeperkingNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Toegangsbeperking.ToString(),
                Attribuutwaarde = string.Empty,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "ToegangsbeperkingNietCorrect", null);
    }
}
