namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

public class WhenNationaleWegen : ChangeAttributesTestBase
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
                NationaleWegen =
                [
                    new ChangeAttributeNationalRoad
                    {
                        Ident2 = Fixture.Create<NationalRoadNumber>().ToString()
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
                NationaleWegen =
                [
                    new ChangeAttributeNationalRoad { Ident2 = "N1" },
                    new ChangeAttributeNationalRoad { Ident2 = "N1" }
                ]
            }
        }, "NationaleWegenNietUniek", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WithLegeIdent8_ThenBadRequest(string ident2)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                NationaleWegen =
                [
                    new ChangeAttributeNationalRoad { Ident2 = ident2 }
                ]
            }
        }, "NationaleWegIdent2Verplicht", null);
    }

    [Fact]
    public async Task WithIdent2NietCorrect_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                NationaleWegen =
                [
                    new ChangeAttributeNationalRoad { Ident2 = "-" }
                ]
            }
        }, "NationaleWegIdent2NietCorrect", null);
    }
}
