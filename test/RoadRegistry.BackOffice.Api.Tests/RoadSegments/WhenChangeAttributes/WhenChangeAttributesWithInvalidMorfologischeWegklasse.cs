namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using BackOffice.Abstractions.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidMorfologischeWegklasse : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidMorfologischeWegklasse(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task MorfologischeWegklasse_MorfologischeWegklasseVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.MorfologischeWegklasse.ToString(),
                Attribuutwaarde = null,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "MorfologischeWegklasseVerplicht", null);
    }

    [Fact]
    public async Task MorfologischeWegklasse_MorfologischeWegklasseNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.MorfologischeWegklasse.ToString(),
                Attribuutwaarde = string.Empty,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "MorfologischeWegklasseNietCorrect", null);
    }
}
