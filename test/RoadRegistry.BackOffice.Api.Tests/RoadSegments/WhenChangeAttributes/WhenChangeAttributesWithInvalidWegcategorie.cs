namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using BackOffice.Abstractions.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegcategorie : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegcategorie(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Wegcategorie_WegcategorieVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegcategorie.ToString(),
                Attribuutwaarde = null,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "WegcategorieVerplicht", null);
    }

    [Fact]
    public async Task Wegcategorie_WegcategorieNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegcategorie.ToString(),
                Attribuutwaarde = string.Empty,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "WegcategorieNietCorrect", null);
    }
}
