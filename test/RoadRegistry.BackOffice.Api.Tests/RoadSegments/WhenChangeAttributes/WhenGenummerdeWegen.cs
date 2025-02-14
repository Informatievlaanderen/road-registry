namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

public class WhenGenummerdeWegen : ChangeAttributesTestBase
{
    [Fact]
    public async Task ThenAcceptedResult()
    {
        // Arrange
        await GivenRoadNetwork();

        var request = new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = Fixture.Create<NumberedRoadNumber>().ToString(),
                        Richting = Fixture.Create<RoadSegmentNumberedRoadDirection>().ToDutchString(),
                        Volgnummer = Fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToString()
                    }
                ],
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WithNietUniek_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
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
                ]
            }
        }, "GenummerdeWegenNietUniek", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WithLegeIdent8_ThenBadRequest(string ident8)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = ident8,
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = "1"
                    }
                ]
            }
        }, "GenummerdeWegIdent8Verplicht", null);
    }

    [Fact]
    public async Task WithIdent8NietCorrect_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "-",
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = "1"
                    }
                ]
            }
        }, "GenummerdeWegIdent8NietCorrect", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WithLegeRichting_ThenBadRequest(string richting)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "A0010001",
                        Richting = richting,
                        Volgnummer = "1"
                    }
                ]
            }
        }, "GenummerdeWegRichtingVerplicht", null);
    }

    [Fact]
    public async Task WithRichtingNietCorrect_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "A0010001",
                        Richting = "-",
                        Volgnummer = "1"
                    }
                ]
            }
        }, "GenummerdeWegRichtingNietCorrect", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WithLeegVolgnummer_ThenBadRequest(string volgnummer)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "A0010001",
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = volgnummer
                    }
                ]
            }
        }, "GenummerdeWegVolgnummerVerplicht", null);
    }

    [Theory]
    [InlineData("-")]
    [InlineData("a")]
    [InlineData("-1")]
    public async Task WithVolgnummerNietCorrect_ThenBadRequest(string volgnummer)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "A0010001",
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = volgnummer
                    }
                ]
            }
        }, "GenummerdeWegVolgnummerNietCorrect", null);
    }
}
