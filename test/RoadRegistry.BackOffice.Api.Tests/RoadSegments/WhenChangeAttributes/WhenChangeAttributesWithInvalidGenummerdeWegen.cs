namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidGenummerdeWegen : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidGenummerdeWegen(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task GenummerdeWegen_NietUniek()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] {
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "A0010001",
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = "1"
                    },
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "A0010001",
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = "1"
                    }
                }
            }
        }, "GenummerdeWegenNietUniek", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GenummerdeWegen_Ident8Verplicht(string ident8)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] { new ChangeAttributeNumberedRoad
                {
                    Ident8 = ident8,
                    Richting = "gelijklopend met de digitalisatiezin",
                    Volgnummer = "1"
                } }
            }
        }, "GenummerdeWegIdent8Verplicht", null);
    }

    [Fact]
    public async Task GenummerdeWegen_Ident8NietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] { new ChangeAttributeNumberedRoad
                {
                    Ident8 = "-",
                    Richting = "gelijklopend met de digitalisatiezin",
                    Volgnummer = "1"
                } }
            }
        }, "GenummerdeWegIdent8NietCorrect", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GenummerdeWegen_RichtingVerplicht(string richting)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] { new ChangeAttributeNumberedRoad
                {
                    Ident8 = "A0010001",
                    Richting = richting,
                    Volgnummer = "1"
                } }
            }
        }, "GenummerdeWegRichtingVerplicht", null);
    }

    [Fact]
    public async Task GenummerdeWegen_RichtingNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] { new ChangeAttributeNumberedRoad
                {
                    Ident8 = "A0010001",
                    Richting = "-",
                    Volgnummer = "1"
                } }
            }
        }, "GenummerdeWegRichtingNietCorrect", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GenummerdeWegen_VolgnummerVerplicht(string volgnummer)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] { new ChangeAttributeNumberedRoad
                {
                    Ident8 = "A0010001",
                    Richting = "gelijklopend met de digitalisatiezin",
                    Volgnummer = volgnummer
                } }
            }
        }, "GenummerdeWegVolgnummerVerplicht", null);
    }

    [Theory]
    [InlineData("-")]
    [InlineData("a")]
    [InlineData("-1")]
    public async Task GenummerdeWegen_VolgnummerNietCorrect(string volgnummer)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id },
                GenummerdeWegen = new[] { new ChangeAttributeNumberedRoad
                {
                    Ident8 = "A0010001",
                    Richting = "gelijklopend met de digitalisatiezin",
                    Volgnummer = volgnummer
                } }
            }
        }, "GenummerdeWegVolgnummerNietCorrect", null);
    }
}
