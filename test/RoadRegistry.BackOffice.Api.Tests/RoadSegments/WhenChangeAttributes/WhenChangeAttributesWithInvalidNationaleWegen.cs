namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidNationaleWegen : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidNationaleWegen(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task NationaleWegen_NietUniek()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                NationaleWegen = new[]
                {
                    new ChangeAttributeNationalRoad { Ident2 = "N1" },
                    new ChangeAttributeNationalRoad { Ident2 = "N1" }
                }
            }
        }, "NationaleWegenNietUniek", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task NationaleWegen_Ident2Verplicht(string ident2)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                NationaleWegen = new[]
                {
                    new ChangeAttributeNationalRoad { Ident2 = ident2 }
                }
            }
        }, "NationaleWegIdent2Verplicht", null);
    }

    [Fact]
    public async Task NationaleWegen_Ident2NietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                NationaleWegen = new[]
                {
                    new ChangeAttributeNationalRoad { Ident2 = "-" }
                }
            }
        }, "NationaleWegIdent2NietCorrect", null);
    }
}
