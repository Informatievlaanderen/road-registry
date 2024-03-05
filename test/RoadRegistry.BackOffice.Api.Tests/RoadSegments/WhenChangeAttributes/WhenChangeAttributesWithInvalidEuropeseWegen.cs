namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidEuropeseWegen : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidEuropeseWegen(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task EuropeseWegen_NietUniek()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                EuropeseWegen = new[]
                {
                    new ChangeAttributeEuropeanRoad { EuNummer = "E40" },
                    new ChangeAttributeEuropeanRoad { EuNummer = "E40" }
                }
            }
        }, "EuropeseWegenNietUniek", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task EuropeseWegen_EuNummerVerplicht(string nummer)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                EuropeseWegen = new[]
                {
                    new ChangeAttributeEuropeanRoad { EuNummer = nummer }
                }
            }
        }, "EuropeseWegEuNummerVerplicht", null);
    }

    [Fact]
    public async Task EuropeseWegen_EuNummerNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                EuropeseWegen = new[]
                {
                    new ChangeAttributeEuropeanRoad { EuNummer = "-" }
                }
            }
        }, "EuropeseWegEuNummerNietCorrect", null);
    }
}
